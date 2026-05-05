namespace AIServices.Models.Options;

/// <summary>
/// Configures the default AI model and any alternative models available to the service.
/// </summary>
public record AIModelsOptions
{
    /// <summary>
    /// Gets the alternative AI models that can be selected instead of the default model.
    /// </summary>
    public List<AIModel> AlternativeModels { get; init; } = [];

    /// <summary>
    /// Gets the model used when no specific model is requested.
    /// </summary>
    public required AIModel DefaultModel { get; init; }
}
