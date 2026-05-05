namespace AIServices.Models;

/// <summary>
/// Represents the result of a text AI operation.
/// </summary>
/// <typeparam name="T">The response payload type.</typeparam>
public record TextAIResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the AI operation completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the parsed or typed response payload.
    /// </summary>
    public required T? Response { get; init; }

    /// <summary>
    /// Gets the raw response text returned by the AI service.
    /// </summary>
    public required string? RawResponse { get; init; }

    /// <summary>
    /// Gets an unsuccessful response with no response content.
    /// </summary>
    public static TextAIResponse<T> NullResponse => new()
    {
        IsSuccess = false,
        RawResponse = null,
        Response = default,
    };
}
