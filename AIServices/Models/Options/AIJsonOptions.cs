using System.Text.Json;
using System.Text.Json.Schema;

namespace AIServices.Models.Options;

public record AIJsonOptions
{
    public required JsonSerializerOptions JsonSerializerOptions { get; set; }
    public required JsonSchemaExporterOptions JsonSchemaExporterOptions { get; set; }
}