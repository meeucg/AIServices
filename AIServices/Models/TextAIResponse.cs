namespace AIServices.Models;

public record TextAIResponse<T>
{
    public required bool IsSuccess { get; init; }
    public required T? Response { get; init; }
    public required string? RawResponse { get; init; }

    public static TextAIResponse<T> NullResponse => new()
    {
        IsSuccess = false,
        RawResponse = null,
        Response = default,
    };
}