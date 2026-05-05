using AIServices.Entities;

namespace AIServices.Models;

public record TextAIRequest
{
    public required Chat ChatContext { get; init; }
    public required string RequestText { get; init; }
}