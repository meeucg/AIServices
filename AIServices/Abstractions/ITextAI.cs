using AIServices.Entities;
using AIServices.Models;

namespace AIServices.Abstractions;

/// <summary>
/// Provides text-based AI completion operations for one-shot prompts and chat conversations.
/// </summary>
public interface ITextAI
{
    /// <summary>
    /// One request, one response, no chat context.
    /// </summary>
    /// <param name="request">Request for an AI model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A response of an AI model</returns>
    Task<TextAIResponse<string>> OneShotResponse(
        string request, CancellationToken ct = default);

    /// <summary>
    /// Chat context is used in generating an answer to a request.
    /// Important : the method doesn't add a ChatPair to the chat
    /// </summary>
    /// <param name="request">A request that contains both chat history and a current request message</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A response of an AI model</returns>
    Task<TextAIResponse<string>> CompleteChat(
        TextAIRequest request, CancellationToken ct = default);

    /// <summary>
    /// One request, one response, no chat context.
    /// </summary>
    /// <param name="request">Request for an AI model</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A strongly typed response of an AI model</returns>
    Task<TextAIResponse<T>> OneShotResponseTyped<T>(
        string request, CancellationToken ct = default);

    /// <summary>
    /// Chat context is used in generating an answer to a request.
    /// Important : the method doesn't add a ChatPair to the chat
    /// </summary>
    /// <param name="request">A request that contains both chat history and a current request message</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A strongly typed response of an AI model</returns>
    Task<TextAIResponse<T>> CompleteChatTyped<T>(
        TextAIRequest request, CancellationToken ct = default);
}
