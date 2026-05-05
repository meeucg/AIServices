namespace AIServices.Abstractions;

/// <summary>
/// Represents a validator that can report the model type it validates for AI requests or responses.
/// </summary>
public interface IValidatorForAI
{
    /// <summary>
    /// General abstraction for grouping typed Validators
    /// </summary>
    /// <returns>A type for which a validator does validation</returns>
    Type GetValidatorType();
}

/// <summary>
/// Validates values of a specific type before or after AI processing.
/// </summary>
/// <typeparam name="T">The type validated by this validator.</typeparam>
public interface IValidatorForAI<in T> : IValidatorForAI
{
    /// <summary>
    /// Validates a certain type, used in TextAI
    /// </summary>
    /// <param name="request">An object to validate</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if everything is ok, false if a problem is found</returns>
    ValueTask<bool> ValidateAsync(T? request, CancellationToken cancellationToken);
}
