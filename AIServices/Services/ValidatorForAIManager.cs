using AIServices.Abstractions;
using Microsoft.Extensions.Logging;

namespace AIServices.Services;

public class ValidatorForAIManager : IValidatorForAIManager
{
    private readonly Dictionary<Type, IValidatorForAI> _validators = new();

    public ValidatorForAIManager(IEnumerable<IValidatorForAI> validators, ILogger<ValidatorForAIManager> logger)
    {
        foreach (var validator in validators)
        {
            var add = _validators.TryAdd(validator.GetValidatorType(), validator);
            if(!add) logger?.LogWarning(
                "Cannot add more than one validator for type: {validatorType}", 
                validator.GetType());
        }
    }

    public IValidatorForAI<T>? GetValidatorFor<T>()
    {
        return _validators.TryGetValue(typeof(T), out var validator) ? (IValidatorForAI<T>)validator : null;
    }
}