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

    public string GetJsonScheme<T>()
    {
        var schema = _jsonSerOpt.GetJsonSchemaAsNode(
            typeof(T),
            _jsonExpOpt);
        return schema.ToJsonString(_jsonSerOpt);
    }

    public BinaryData GetBinaryJsonScheme<T>()
    {
        var schema = _jsonSerOpt.GetJsonSchemaAsNode(
            typeof(T),
            _jsonExpOpt);
        return BinaryData.FromString(schema.ToJsonString(_jsonSerOpt));
    }
}