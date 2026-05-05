using AIServices.Entities;

namespace AIServices.Models;

/// <summary>
/// Represents a text AI request together with the chat context used to answer it.
/// </summary>
public record TextAIRequest
{
    /// <summary>
    /// Gets the chat context that should be included in completion generation.
    /// </summary>
    public required Chat ChatContext { get; init; }

    /// <summary>
    /// Gets the current user request text.
    /// </summary>
    public required string RequestText { get; init; }
}
