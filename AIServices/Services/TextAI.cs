using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using AIServices.Abstractions;
using AIServices.Entities;
using AIServices.Models;
using AIServices.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AIServices.Services;

public class TextAI : ITextAI
{
    private readonly AIModel _model;
    private readonly IJsonSchemaHelper _jsonSchemaHelper;
    private readonly JsonSerializerOptions _jsonSerOpt;
    private readonly TextAIOptions _textAIOptions;
    private readonly IValidatorForAIManager _validatorForAIManager;
    private readonly ILogger<TextAI>? _logger;

    private readonly ChatClient _client;
    private delegate Task<TextAIResponse<T>> TextAIResponseHandler<T>(
        TextAIRequest request, CancellationToken cancellationToken);

    public TextAI(
        IValidatorForAIManager validatorForAIManager,
        IJsonSchemaHelper jsonSchemaHelper,
        IOptions<AIJsonOptions> aiJsonOptions,
        IOptions<TextAIOptions> opt, 
        AIModel model,
        ILogger<TextAI>? logger = null
        )
    {
        _model = model;
        _jsonSchemaHelper = jsonSchemaHelper;
        _jsonSerOpt = aiJsonOptions.Value.JsonSerializerOptions;
        _textAIOptions = opt.Value;
        _validatorForAIManager = validatorForAIManager;
        _logger = logger;
        
        _client = new ChatClient(
            _model.ModelName,
            new ApiKeyCredential(_textAIOptions.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(_textAIOptions.ApiEndpoint)
            }
        );
    }

    private static List<ChatMessage> ConvertChatMessages(Chat chat)
    {
        var result = new List<ChatMessage>();
        if (chat.InitialState != null)
        {
            if (chat.InitialState.SystemPrompt != null)
                result.Add(new SystemChatMessage(chat.InitialState.SystemPrompt));
            
            if(chat.InitialState.InitialAssistantMessage != null)
                result.Add(new AssistantChatMessage(chat.InitialState.InitialAssistantMessage));
        }

        var tail = chat.GetChatHistory()
            .SelectMany(chatPair => new ChatMessage[]
            {
                new UserChatMessage(chatPair.Request),
                new AssistantChatMessage(chatPair.Response)
            })
            .ToList();
        
        result.AddRange(tail);
        return result;
    }

    private async Task<string?> CompleteChatProtocolAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatResponseFormat? responseFormat = null,
        CancellationToken ct = default)
    {
        using var content = TextAIBinaryHelper.GetBinaryRequest(
            _model,
            messages,
            _jsonSerOpt,
            responseFormat,
            ct);

        var result = await _client.CompleteChatAsync(
            content,
            new RequestOptions
            {
                CancellationToken = ct
            });

        return TextAIBinaryHelper.GetRawResponseFromBinary(
            result.GetRawResponse().Content,
            ct);
    }

    private async Task<TextAIResponse<string>> TryCompleteChat(
        TextAIRequest request, CancellationToken ct = default)
    {
        var convertedHistory = ConvertChatMessages(request.ChatContext);
        convertedHistory.Add(new UserChatMessage(request.RequestText));

        var response = await CompleteChatProtocolAsync(
            convertedHistory,
            ct: ct);

        return new TextAIResponse<string>
        {
            IsSuccess = true,
            RawResponse = response,
            Response = response,
        };
    }

    private async Task<TextAIResponse<T>> TryCompleteChatTyped<T>(
        TextAIRequest request, CancellationToken ct = default)
    {
        if(!_model.SupportsJsonOutput) return TextAIResponse<T>.NullResponse;
        var convertedHistory = ConvertChatMessages(request.ChatContext);
        convertedHistory.Add(new UserChatMessage(request.RequestText));

        var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            typeof(T).Name + "Scheme",
            _jsonSchemaHelper.GetBinaryJsonScheme<T>(),
            jsonSchemaIsStrict: true);

        var response = await CompleteChatProtocolAsync(
            convertedHistory,
            responseFormat,
            ct);
        
        if(response == null) return TextAIResponse<T>.NullResponse;
        var result = JsonSerializer.Deserialize<T>(response, _jsonSerOpt);
        
        return new TextAIResponse<T>
        {
            IsSuccess = true,
            Response = result,
            RawResponse = response,
        };
    }
    
    
    private async Task<TextAIResponse<T>> CompleteChatSecure<T>(
        TextAIResponseHandler<T> handler, TextAIRequest request, CancellationToken ct = default)
    {
        for (var i = 0; i < _textAIOptions.RetryCount; i++)
        {
            using var timeoutCts = new CancellationTokenSource(_textAIOptions.RetryAfter);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                ct,
                timeoutCts.Token);

            timeoutCts.Token.Register(_ =>
            {
                _logger?.LogWarning(
                    "AI request for chat with id : {chat_id} timed out", 
                    request.ChatContext.Id);
            }, null);

            try
            {
                var result = await handler(request, linkedCts.Token);
                if (!result.IsSuccess)
                {
                    _logger?.LogWarning(
                        "AI request for chat with id : {chat_id} failed at attempt number {iteration}",
                        request.ChatContext.Id,
                        i);
                }

                var validator = _validatorForAIManager.GetValidatorFor<T>();
                if (validator == null) return result;

                var validationResult = await validator.ValidateAsync(
                    result.Response, linkedCts.Token);
                if (validationResult) return result;

                _logger?.LogWarning(
                    "AI request for chat with id : {chat_id} failed validation for type {type}",
                    request.ChatContext.Id,
                    typeof(T).Name);
            }
            catch (OperationCanceledException)
            {
                if (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
                {
                    continue;
                }
                
                _logger?.LogInformation(
                    "AI request for chat with id : {chat_id} was canceled",
                    request.ChatContext.Id);
                return TextAIResponse<T>.NullResponse;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    "AI request for chat with id : {chat_id} failed at attempt number {iteration}" +
                    "with Exception message:\n {ex},\n Stack trace:\n {trace}",
                    request.ChatContext.Id,
                    i,
                    ex.Message,
                    ex.StackTrace);
            }
        }
        _logger?.LogError("AI request for chat with id : {chat_id} " +
                          "failed at all attempts, returning null response", request.ChatContext.Id);
        return TextAIResponse<T>.NullResponse;
    }
    
    public async Task<TextAIResponse<string>> OneShotResponse(
        string request, CancellationToken ct = default)
    {
        var fullRequest = new TextAIRequest
        {
            ChatContext = new Chat(Guid.Empty),
            RequestText = request,
        };
        return await CompleteChatSecure(TryCompleteChat, fullRequest, ct);
    }

    public async Task<TextAIResponse<string>> CompleteChat(
        TextAIRequest request, CancellationToken ct = default)
    {
        return await CompleteChatSecure(TryCompleteChat, request, ct);
    }

    public async Task<TextAIResponse<T>> OneShotResponseTyped<T>(
        string request, CancellationToken ct = default)
    {
        var fullRequest = new TextAIRequest
        {
            ChatContext = new Chat(Guid.Empty),
            RequestText = request,
        };
        return await CompleteChatSecure(TryCompleteChatTyped<T>, fullRequest, ct);
    }

    public async Task<TextAIResponse<T>> CompleteChatTyped<T>(
        TextAIRequest request, CancellationToken ct = default)
    {
        return await CompleteChatSecure(TryCompleteChatTyped<T>, request, ct);
    }
}
