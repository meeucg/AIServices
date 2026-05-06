using System.ComponentModel;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AIServices.Abstractions;
using AIServices.Models;
using AIServices.Models.Options;
using AIServices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIServices.ServiceBuilders;

public static class AIServicesServiceBuilder
{
    public static IServiceCollection AddAIServices(
        this IServiceCollection services,
        IConfigurationSection textAIOptionsSection,
        IConfigurationSection aiModelsOptionsSection,
        Action<AIJsonOptions>? configureAIJsonOptions = null)
    {
        services.AddSingleton<IValidatorForAIManager, ValidatorForAIManager>();
        services.AddSingleton<IJsonSchemaHelper, JsonSchemaHelper>();

        services.AddOptions<TextAIOptions>()
            .Bind(textAIOptionsSection);

        services.AddSingleton<IOptions<AIModelsOptions>>(_ =>
            Options.Create(BindAIModelsOptions(aiModelsOptionsSection)));

        var aiJsonOptionsBuilder = services.AddOptions<AIJsonOptions>();

        aiJsonOptionsBuilder.Configure(configureAIJsonOptions ?? ConfigureDefaultAIJsonOptions);

        services.AddSingleton<ITextAI>(sp => new TextAI(
            sp.GetRequiredService<IValidatorForAIManager>(),
            sp.GetRequiredService<IJsonSchemaHelper>(),
            sp.GetRequiredService<IOptions<AIJsonOptions>>(),
            sp.GetRequiredService<IOptions<TextAIOptions>>(),
            sp.GetRequiredService<IOptions<AIModelsOptions>>().Value.DefaultModel,
            sp.GetService<ILogger<TextAI>>()));

        RegisterKeyedTextAIs(services, aiModelsOptionsSection);

        return services;
    }

    private static void RegisterKeyedTextAIs(
        IServiceCollection services,
        IConfigurationSection aiModelsOptionsSection)
    {
        var aiModelsOptions = BindAIModelsOptions(aiModelsOptionsSection);

        var allModels = new List<AIModel> { aiModelsOptions.DefaultModel };
        
        allModels.AddRange(aiModelsOptions.AlternativeModels);

        foreach (var model in allModels
                     .GroupBy(x => x.ModelAlias, StringComparer.Ordinal)
                     .Select(g => g.First()))
        {
            services.AddKeyedSingleton<ITextAI>(model.ModelAlias, (sp, _) => new TextAI(
                sp.GetRequiredService<IValidatorForAIManager>(),
                sp.GetRequiredService<IJsonSchemaHelper>(),
                sp.GetRequiredService<IOptions<AIJsonOptions>>(),
                sp.GetRequiredService<IOptions<TextAIOptions>>(),
                model,
                sp.GetService<ILogger<TextAI>>()));
        }
    }

    private static AIModelsOptions BindAIModelsOptions(IConfigurationSection aiModelsOptionsSection)
    {
        var defaultModelSection = aiModelsOptionsSection.GetSection(nameof(AIModelsOptions.DefaultModel));

        if (!defaultModelSection.Exists())
            throw new InvalidOperationException("Default AI model configuration section is missing.");

        return new AIModelsOptions
        {
            DefaultModel = BindAIModel(defaultModelSection),
            AlternativeModels = aiModelsOptionsSection
                .GetSection(nameof(AIModelsOptions.AlternativeModels))
                .GetChildren()
                .Select(BindAIModel)
                .ToList()
        };
    }

    private static AIModel BindAIModel(IConfigurationSection aiModelSection)
    {
        return new AIModel
        {
            ModelAlias = aiModelSection[nameof(AIModel.ModelAlias)]
                         ?? throw new InvalidOperationException("AI model alias is missing."),
            ModelName = aiModelSection[nameof(AIModel.ModelName)]
                        ?? throw new InvalidOperationException("AI model name is missing."),
            SupportsJsonOutput = aiModelSection.GetValue<bool>(nameof(AIModel.SupportsJsonOutput)),
            SupportsFunctionCalling = aiModelSection.GetValue<bool>(nameof(AIModel.SupportsFunctionCalling)),
            RequestBodyExtensions = BindJsonObject(
                aiModelSection.GetSection(nameof(AIModel.RequestBodyExtensions)))
        };
    }

    private static JsonObject BindJsonObject(IConfigurationSection section)
    {
        var result = new JsonObject();

        foreach (var child in section.GetChildren())
            result[child.Key] = BindJsonNode(child);

        return result;
    }

    private static JsonNode? BindJsonNode(IConfigurationSection section)
    {
        var children = section.GetChildren().ToList();

        if (children.Count == 0)
            return BindJsonValue(section.Value);

        if (children.All(child => int.TryParse(child.Key, out _)))
        {
            var array = new JsonArray();

            foreach (var child in children.OrderBy(child => int.Parse(child.Key)))
                array.Add(BindJsonNode(child));

            return array;
        }

        var obj = new JsonObject();

        foreach (var child in children)
            obj[child.Key] = BindJsonNode(child);

        return obj;
    }

    private static JsonNode? BindJsonValue(string? value)
    {
        if (value is null)
            return null;

        if (bool.TryParse(value, out var boolValue))
            return JsonValue.Create(boolValue);

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
            return JsonValue.Create(longValue);

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            return JsonValue.Create(doubleValue);

        try
        {
            return JsonNode.Parse(value);
        }
        catch (JsonException)
        {
            return JsonValue.Create(value);
        }
    }

    private static void ConfigureDefaultAIJsonOptions(AIJsonOptions o)
    {
        o.JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        o.JsonSchemaExporterOptions = new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) =>
            {
                var attributeProvider =
                    context.PropertyInfo is not null
                        ? context.PropertyInfo.AttributeProvider
                        : context.TypeInfo.Type;

                var descriptionAttr = attributeProvider?
                    .GetCustomAttributes(typeof(DescriptionAttribute), inherit: true)
                    .OfType<DescriptionAttribute>()
                    .FirstOrDefault();

                if (descriptionAttr is null)
                    return schema;

                var obj = schema as JsonObject ?? new JsonObject();
                obj["description"] = descriptionAttr.Description;
                return obj;
            }
        };
    }
}
