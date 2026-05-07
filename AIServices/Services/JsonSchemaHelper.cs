using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Schema;
using AIServices.Abstractions;
using AIServices.Models.Options;
using Microsoft.Extensions.Options;

namespace AIServices.Services;

public class JsonSchemaHelper(IOptions<AIJsonOptions> opt) : IJsonSchemaHelper
{
    private readonly JsonSerializerOptions _jsonSerOpt = opt.Value.JsonSerializerOptions;
    private readonly JsonSchemaExporterOptions _jsonExpOpt = opt.Value.JsonSchemaExporterOptions;
    private readonly ConcurrentDictionary<Type, CachedJsonSchema> _schemaCache = new();

    public string GetJsonScheme<T>()
    {
        return GetCachedSchema<T>().JsonSchema;
    }

    public BinaryData GetBinaryJsonScheme<T>()
    {
        return GetCachedSchema<T>().BinaryJsonSchema;
    }

    private CachedJsonSchema GetCachedSchema<T>()
    {
        return _schemaCache.GetOrAdd(typeof(T), CreateSchema);
    }

    private CachedJsonSchema CreateSchema(Type type)
    {
        var schema = _jsonSerOpt.GetJsonSchemaAsNode(
            type,
            _jsonExpOpt);
        var jsonSchema = schema.ToJsonString(_jsonSerOpt);

        return new CachedJsonSchema(
            jsonSchema,
            BinaryData.FromString(jsonSchema));
    }

    private sealed record CachedJsonSchema(
        string JsonSchema,
        BinaryData BinaryJsonSchema);
}
