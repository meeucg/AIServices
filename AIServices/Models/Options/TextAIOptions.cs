namespace AIServices.Models.Options;

public record TextAIOptions
{
    public required string ApiKey { get; set; }
    public required string ApiEndpoint { get; set; }
    public TimeSpan RetryAfter { get; set; } = TimeSpan.FromMinutes(3);
    public int RetryCount { get; set; } = 3;
}