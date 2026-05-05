namespace AIServices.Models;

public record AIModel
{
    public required string ModelAlias { get; init; }
    public required string ModelName { get; init; }
    public bool SupportsJsonOutput { get; init; }
    public bool SupportsFunctionCalling { get; init; }
}