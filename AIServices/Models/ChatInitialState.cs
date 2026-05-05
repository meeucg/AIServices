namespace AIServices.Models;

public record ChatInitialState
{
    public string? SystemPrompt { get; init; }
    public string? InitialAssistantMessage { get; init; }
}