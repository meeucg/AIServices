using System.Text.Json.Nodes;

namespace AIServices.Models;

/// <summary>
/// Describes an AI model and the capabilities exposed by the configured provider.
/// </summary>
public record AIModel
{
    /// <summary>
    /// Gets the application-level alias used to reference the model.
    /// </summary>
    public required string ModelAlias { get; init; }

    /// <summary>
    /// Gets the provider-specific model name sent to the AI service.
    /// </summary>
    public required string ModelName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the model can produce JSON-formatted output.
    /// </summary>
    public bool SupportsJsonOutput { get; init; }

    /// <summary>
    /// Gets a value indicating whether the model supports function calling.
    /// </summary>
    public bool SupportsFunctionCalling { get; init; }

    /// <summary>
    /// Gets additional JSON fields that should be merged into the OpenAI request body for this model.
    /// </summary>
    public JsonObject RequestBodyExtensions { get; init; } = [];
}
