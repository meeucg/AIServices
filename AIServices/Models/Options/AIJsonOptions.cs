using System.Text.Json;
using System.Text.Json.Schema;

namespace AIServices.Models.Options;

/// <summary>
/// Configures JSON serialization and schema generation used by AI services.
/// </summary>
public record AIJsonOptions
{
    /// <summary>
    /// Gets or sets the serializer options used for AI request and response JSON.
    /// </summary>
    public required JsonSerializerOptions JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the exporter options used when generating JSON schemas.
    /// </summary>
    public required JsonSchemaExporterOptions JsonSchemaExporterOptions { get; set; }
}
