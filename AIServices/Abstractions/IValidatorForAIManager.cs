namespace AIServices.Abstractions;

/// <summary>
/// Resolves registered AI validators for strongly typed request or response models.
/// </summary>
public interface IValidatorForAIManager
{
    /// <summary>
    /// Gets the validator registered for the specified type, if one is available.
    /// </summary>
    /// <typeparam name="T">The type that needs validation.</typeparam>
    /// <returns>The matching validator, or <c>null</c> when no validator is registered.</returns>
    IValidatorForAI<T>? GetValidatorFor<T>();
}
