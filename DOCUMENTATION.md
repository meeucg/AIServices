# AIServices Documentation

Generated from the project's XML documentation summaries.

## Assembly

`AIServices`

## Abstractions

### `AIServices.Abstractions.IJsonSchemaHelper`

Provides JSON schema generation helpers for strongly typed AI responses.

#### `GetJsonScheme<T>()`

Uses `JsonSerializationOptions` and `JsonSchemaExporterOptions` registered as `IOptions` from dependency injection. Configure those options to change behavior.

Returns a JSON schema for type `T`.

#### `GetBinaryJsonScheme<T>()`

Same as `GetJsonScheme`, but returns `BinaryData`. Useful for the OpenAI SDK.

Returns a JSON schema for type `T` in `BinaryData` format.

### `AIServices.Abstractions.ITextAI`

Provides text-based AI completion operations for one-shot prompts and chat conversations.

#### `OneShotResponse(string request, CancellationToken ct = default)`

One request, one response, no chat context.

Returns a text response from an AI model.

#### `CompleteChat(TextAIRequest request, CancellationToken ct = default)`

Uses chat context when generating an answer to a request.

Important: this method does not add a `ChatPair` to the chat.

Returns a text response from an AI model.

#### `OneShotResponseTyped<T>(string request, CancellationToken ct = default)`

One request, one response, no chat context.

Returns a strongly typed response from an AI model.

#### `CompleteChatTyped<T>(TextAIRequest request, CancellationToken ct = default)`

Uses chat context when generating an answer to a request.

Important: this method does not add a `ChatPair` to the chat.

Returns a strongly typed response from an AI model.

### `AIServices.Abstractions.IValidatorForAI`

Represents a validator that can report the model type it validates for AI requests or responses.

#### `GetValidatorType()`

General abstraction for grouping typed validators.

Returns the type validated by the validator.

### `AIServices.Abstractions.IValidatorForAI<T>`

Validates values of a specific type before or after AI processing.

#### `ValidateAsync(T? request, CancellationToken cancellationToken)`

Validates a certain type for use in `TextAI`.

Returns `true` when validation succeeds, or `false` when a problem is found.

### `AIServices.Abstractions.IValidatorForAIManager`

Resolves registered AI validators for strongly typed request or response models.

#### `GetValidatorFor<T>()`

Gets the validator registered for the specified type, if one is available.

Returns the matching validator, or `null` when no validator is registered.

## Entities

### `AIServices.Entities.Chat`

Represents a chat conversation with optional initial state and completed request-response pairs.

Constructor parameters:

- `id`: The chat identifier, or `null` to create a new identifier.
- `chatPairs`: The existing chat history to initialize with.
- `chatInitialState`: The optional initial state used to seed the chat.

#### `InitialState`

Gets the optional initial state that seeds the chat.

#### `Id`

Gets the unique identifier of the chat.

#### `AddChatPair(ChatPair message)`

Adds a completed request-response pair to the chat history.

Throws `ArgumentNullException` when `message` is `null`.

#### `GetChatHistory()`

Gets the completed chat history in insertion order.

Returns the read-only list of completed chat pairs.

## Models

### `AIServices.Models.AIModel`

Describes an AI model and the capabilities exposed by the configured provider.

Properties:

- `ModelAlias`: Gets the application-level alias used to reference the model.
- `ModelName`: Gets the provider-specific model name sent to the AI service.
- `SupportsJsonOutput`: Gets a value indicating whether the model can produce JSON-formatted output.
- `SupportsFunctionCalling`: Gets a value indicating whether the model supports function calling.
- `RequestBodyExtensions`: Gets additional JSON fields that should be merged into the OpenAI request body for this model.

### `AIServices.Models.ChatInitialState`

Defines optional starting messages used when creating a chat context.

Properties:

- `SystemPrompt`: Gets the optional system prompt that guides the conversation.
- `InitialAssistantMessage`: Gets the optional assistant message used to seed the conversation.

### `AIServices.Models.ChatPair`

Messages in chats are saved in pairs to prevent inconsistency. If an exception happens while generating an AI answer, the pair is not saved in a chat, which keeps all pairs complete in a single `Chat` instance.

Properties:

- `Request`: Client request.
- `Response`: AI response.

### `AIServices.Models.TextAIRequest`

Represents a text AI request together with the chat context used to answer it.

Properties:

- `ChatContext`: Gets the chat context that should be included in completion generation.
- `RequestText`: Gets the current user request text.

### `AIServices.Models.TextAIResponse<T>`

Represents the result of a text AI operation.

Properties:

- `IsSuccess`: Gets a value indicating whether the AI operation completed successfully.
- `Response`: Gets the parsed or typed response payload.
- `RawResponse`: Gets the raw response text returned by the AI service.
- `NullResponse`: Gets an unsuccessful response with no response content.

## Options

### `AIServices.Models.Options.AIJsonOptions`

Configures JSON serialization and schema generation used by AI services.

Properties:

- `JsonSerializerOptions`: Gets or sets the serializer options used for AI request and response JSON.
- `JsonSchemaExporterOptions`: Gets or sets the exporter options used when generating JSON schemas.

### `AIServices.Models.Options.AIModelsOptions`

Configures the default AI model and any alternative models available to the service.

Properties:

- `AlternativeModels`: Gets the alternative AI models that can be selected instead of the default model.
- `DefaultModel`: Gets the model used when no specific model is requested.

### `AIServices.Models.Options.TextAIOptions`

Configures the text AI service connection and retry behavior.

Properties:

- `ApiKey`: Gets or sets the API key used to authenticate with the text AI provider.
- `ApiEndpoint`: Gets or sets the API endpoint used for text AI requests.
- `RetryAfter`: Gets or sets the delay between retry attempts after a failed request.
- `RetryCount`: Gets or sets the maximum number of retry attempts.
