using System.ComponentModel;
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

        services.AddOptions<AIModelsOptions>()
            .Bind(aiModelsOptionsSection);

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
        var aiModelsOptions = aiModelsOptionsSection.Get<AIModelsOptions>()
                              ?? throw new InvalidOperationException("AIModelsOptions configuration section is invalid.");

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