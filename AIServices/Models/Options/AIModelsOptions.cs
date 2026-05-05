namespace AIServices.Models.Options;

public record AIModelsOptions
{
    public List<AIModel> AlternativeModels { get; init; } = [];
    public required AIModel DefaultModel { get; init; }
}