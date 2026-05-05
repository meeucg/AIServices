namespace AIServices.Abstractions;

public interface IValidatorForAI
{
    /// <summary>
    /// General abstraction for grouping typed Validators
    /// </summary>
    /// <returns>A type for which a validator does validation</returns>
    Type GetValidatorType();
}

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