# Meeucg.FitFlow.AIServices

`Meeucg.FitFlow.AIServices` is a .NET library for FitFlow that extends the OpenAI SDK with a more comfortable service layer for text AI workflows and typed outputs.

It provides dependency-injection registration, configurable model aliases, one-shot prompts, chat-context completions, JSON-schema typed responses, retry handling, and optional validators for AI responses.

## Installation

```bash
dotnet add package Meeucg.FitFlow.AIServices --version 1.0.9
```

## Quick Start

### 1. Add Configuration

Add text AI and model configuration to your application settings.

```json
{
  "TextAI": {
    "ApiKey": "<your-api-key>",
    "ApiEndpoint": "https://api.openai.com/v1",
    "RetryAfter": "00:03:00",
    "RetryCount": 3
  },
  "AIModels": {
    "DefaultModel": {
      "ModelAlias": "default",
      "ModelName": "<openai-chat-model>",
      "SupportsJsonOutput": true,
      "SupportsFunctionCalling": false,
      "RequestBodyExtensions": {}
    },
    "AlternativeModels": [
      {
        "ModelAlias": "fast",
        "ModelName": "<openai-chat-model>",
        "SupportsJsonOutput": true,
        "SupportsFunctionCalling": false,
        "RequestBodyExtensions": {}
      }
    ]
  }
}
```

### 2. Register Services

Register AIServices in your DI container.

```csharp
using AIServices.ServiceBuilders;

builder.Services.AddAIServices(
    builder.Configuration.GetSection("TextAI"),
    builder.Configuration.GetSection("AIModels"));
```

### 3. Send a One-Shot Prompt

Use `ITextAI` for a single prompt without chat history.

```csharp
using AIServices.Abstractions;

app.MapPost("/ask", async (ITextAI textAI, string prompt) =>
{
    var result = await textAI.OneShotResponse(prompt);

    return result.IsSuccess
        ? Results.Ok(result.Response)
        : Results.Problem("The AI request failed.");
});
```

## Chat Context

Use `Chat` when the response should consider previous messages or an initial system prompt.

```csharp
using AIServices.Abstractions;
using AIServices.Entities;
using AIServices.Models;

var chat = new Chat(
    chatInitialState: new ChatInitialState
    {
        SystemPrompt = "You are a concise fitness assistant."
    });

var requestText = "Give me a short warm-up for leg day.";

var result = await textAI.CompleteChat(new TextAIRequest
{
    ChatContext = chat,
    RequestText = requestText
});

if (result is { IsSuccess: true, Response: not null })
{
    chat.AddChatPair(new ChatPair
    {
        Request = requestText,
        Response = result.Response
    });
}
```

`CompleteChat` and `CompleteChatTyped<T>` do not add messages to the chat automatically. Add a `ChatPair` yourself after a successful response.

## Typed Responses

Use `OneShotResponseTyped<T>` or `CompleteChatTyped<T>` when you want a strongly typed response. The selected model must have `SupportsJsonOutput` set to `true`.

```csharp
using System.ComponentModel;
using AIServices.Abstractions;

public sealed record WorkoutSuggestion
{
    [Description("A short title for the workout.")]
    public required string Title { get; init; }

    [Description("A list of exercises to perform.")]
    public required IReadOnlyList<string> Exercises { get; init; }

    [Description("Estimated workout duration in minutes.")]
    public required int DurationMinutes { get; init; }
}

var result = await textAI.OneShotResponseTyped<WorkoutSuggestion>(
    "Create a beginner-friendly full-body workout.");

if (result is { IsSuccess: true, Response: not null })
{
    var workout = result.Response;
    Console.WriteLine($"{workout.Title}: {workout.DurationMinutes} minutes");
}
```

`DescriptionAttribute` values are included in generated JSON schemas by the default JSON configuration.

Generated schemas are cached per response type. The cache stores both the JSON string and `BinaryData` representation, so repeated typed calls for the same `T` reuse the same schema payload.

## Validators

Register validators when typed AI responses need extra application-level validation. If a validator is registered for `T`, `TextAI` runs it before returning a successful typed result.

```csharp
using AIServices.Abstractions;

public sealed class WorkoutSuggestionValidator : IValidatorForAI<WorkoutSuggestion>
{
    public Type GetValidatorType() => typeof(WorkoutSuggestion);

    public ValueTask<bool> ValidateAsync(
        WorkoutSuggestion? request,
        CancellationToken cancellationToken)
    {
        var isValid = request is not null
            && request.DurationMinutes > 0
            && request.Exercises.Count > 0;

        return ValueTask.FromResult(isValid);
    }
}
```

Register the validator as the non-generic `IValidatorForAI` abstraction so the validator manager can discover it.

```csharp
using AIServices.Abstractions;

builder.Services.AddSingleton<IValidatorForAI, WorkoutSuggestionValidator>();
```

## Keyed Models

Every configured model alias is registered as a keyed `ITextAI`. Use this when you want to choose a specific configured model.

```csharp
using AIServices.Abstractions;
using Microsoft.Extensions.DependencyInjection;

var fastAI = serviceProvider.GetRequiredKeyedService<ITextAI>("fast");

var result = await fastAI.OneShotResponse("Summarize today's workout plan.");
```

The unkeyed `ITextAI` uses `AIModels:DefaultModel`.

## Additional Request JSON

Some OpenAI-compatible providers expose request fields before the SDK has first-class options for them. Add those fields to `RequestBodyExtensions` on the model configuration. The value can be a nested JSON object or array, not only flat key-value pairs.

```json
{
  "AIModels": {
    "DefaultModel": {
      "ModelAlias": "GPT-OSS 120B",
      "ModelName": "openai/gpt-oss-120b",
      "SupportsJsonOutput": true,
      "SupportsFunctionCalling": true,
      "RequestBodyExtensions": {
        "reasoning_effort": "minimal",
        "metadata": {
          "source": "fitflow",
          "features": ["typed-output", "interview"]
        }
      }
    }
  }
}
```

`TextAI` builds requests through the OpenAI SDK protocol method so these extra fields are merged into the final JSON body while SDK models still serialize chat messages and typed JSON response formats.

## JSON Options

`AddAIServices` uses default JSON options that enable camel-case property names, enum strings, relaxed JSON escaping, and schema descriptions from `DescriptionAttribute`.

You can replace the JSON and schema options during registration.

```csharp
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AIServices.Models.Options;
using AIServices.ServiceBuilders;

builder.Services.AddAIServices(
    builder.Configuration.GetSection("TextAI"),
    builder.Configuration.GetSection("AIModels"),
    options =>
    {
        options.JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        options.JsonSchemaExporterOptions = new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true
        };
    });
```

## API Documentation

See [DOCUMENTATION.md](DOCUMENTATION.md) for the generated API documentation based on XML summaries.
