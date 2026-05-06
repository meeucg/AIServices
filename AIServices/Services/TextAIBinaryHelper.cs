using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIServices.Models;
using OpenAI.Chat;

namespace AIServices.Services;

internal static class TextAIBinaryHelper
{
    public static BinaryContent GetBinaryRequest(
        AIModel model,
        IReadOnlyList<ChatMessage> messages,
        JsonSerializerOptions jsonSerializerOptions,
        ChatResponseFormat? responseFormat = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var body = new JsonObject
        {
            ["model"] = model.ModelName,
            ["messages"] = SerializeSdkModels(messages, ct)
        };

        if (responseFormat is not null)
            body["response_format"] = SerializeSdkModel(responseFormat);

        MergeInto(body, model.RequestBodyExtensions, ct);
        ct.ThrowIfCancellationRequested();

        return BinaryContent.Create(
            BinaryData.FromString(body.ToJsonString(jsonSerializerOptions)));
    }

    public static string? GetRawResponseFromBinary(
        BinaryData aiResponse,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var responseJson = JsonDocument.Parse(aiResponse.ToString());
        ct.ThrowIfCancellationRequested();

        return responseJson.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private static JsonArray SerializeSdkModels(
        IEnumerable<ChatMessage> messages,
        CancellationToken ct)
    {
        var result = new JsonArray();

        foreach (var message in messages)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(SerializeSdkModel(message));
        }

        return result;
    }

    private static JsonNode? SerializeSdkModel(object model)
    {
        var binaryData = ModelReaderWriter.Write(
            model,
            ModelReaderWriterOptions.Json);

        return JsonNode.Parse(binaryData.ToString());
    }

    private static void MergeInto(
        JsonObject target,
        JsonObject source,
        CancellationToken ct)
    {
        foreach (var (key, sourceValue) in source)
        {
            ct.ThrowIfCancellationRequested();

            if (sourceValue is JsonObject sourceObject &&
                target[key] is JsonObject targetObject)
            {
                MergeInto(targetObject, sourceObject, ct);
                continue;
            }

            target[key] = sourceValue?.DeepClone();
        }
    }
}
