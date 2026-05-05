namespace AIServices.Models.Options;

/// <summary>
/// Configures the text AI service connection and retry behavior.
/// </summary>
public record TextAIOptions
{
    /// <summary>
    /// Gets or sets the API key used to authenticate with the text AI provider.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API endpoint used for text AI requests.
    /// </summary>
    public required string ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the delay between retry attempts after a failed request.
    /// </summary>
    public TimeSpan RetryAfter { get; set; } = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
