namespace AIServices.Models;

/// <summary>
/// Messages in chats are saved in pairs to prevent inconsistency
/// in case an Exception happens on generating an AI answer,
/// in that case a pair is simply not saved in a Chat,
/// this leads to all pairs being complete in a single Chat instance
/// </summary>
public record ChatPair
{
    /// <summary>
    /// Client's request
    /// </summary>
    public required string Request { get; init; }
    
    /// <summary>
    /// AI's response
    /// </summary>
    public required string Response { get; init; }
}