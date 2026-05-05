using AIServices.Models;

namespace AIServices.Entities;

/// <summary>
/// Represents a chat conversation with optional initial state and completed request-response pairs.
/// </summary>
/// <param name="id">The chat identifier, or <c>null</c> to create a new identifier.</param>
/// <param name="chatPairs">The existing chat history to initialize with.</param>
/// <param name="chatInitialState">The optional initial state used to seed the chat.</param>
public class Chat(
    Guid? id = null, 
    IEnumerable<ChatPair>? chatPairs = null, 
    ChatInitialState? chatInitialState = null)
{
    private readonly List<ChatPair> _chatPairs = chatPairs?.ToList() ?? [];

    /// <summary>
    /// Gets the optional initial state that seeds the chat.
    /// </summary>
    public ChatInitialState? InitialState => chatInitialState;

    /// <summary>
    /// Gets the unique identifier of the chat.
    /// </summary>
    public Guid Id { get; } = id ?? Guid.NewGuid();

    /// <summary>
    /// Adds a completed request-response pair to the chat history.
    /// </summary>
    /// <param name="message">The completed chat pair to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <c>null</c>.</exception>
    public void AddChatPair(ChatPair message) => 
        _chatPairs.Add(message ?? throw new ArgumentNullException(nameof(message)));

    /// <summary>
    /// Gets the completed chat history in insertion order.
    /// </summary>
    /// <returns>The read-only list of completed chat pairs.</returns>
    public IReadOnlyList<ChatPair> GetChatHistory() => _chatPairs;
}
