using AIServices.Models;

namespace AIServices.Entities;

public class Chat(
    Guid? id = null, 
    IEnumerable<ChatPair>? chatPairs = null, 
    ChatInitialState? chatInitialState = null)
{
    private readonly List<ChatPair> _chatPairs = chatPairs?.ToList() ?? [];

    public ChatInitialState? InitialState => chatInitialState;

    public Guid Id { get; } = id ?? Guid.NewGuid();

    public void AddChatPair(ChatPair message) => 
        _chatPairs.Add(message ?? throw new ArgumentNullException(nameof(message)));

    public IReadOnlyList<ChatPair> GetChatHistory() => _chatPairs;
}