namespace AIServices.Abstractions;

public interface IValidatorForAIManager
{
    IValidatorForAI<T>? GetValidatorFor<T>();
}