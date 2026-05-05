namespace AIServices.Models;

/// <summary>
/// Defines optional starting messages used when creating a chat context.
/// </summary>
public record ChatInitialState
{
    /// <summary>
    /// Gets the optional system prompt that guides the conversation.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Gets the optional assistant message used to seed the conversation.
    /// </summary>
    public string? InitialAssistantMessage { get; init; }
}
