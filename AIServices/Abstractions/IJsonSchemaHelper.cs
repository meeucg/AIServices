namespace AIServices.Abstractions;

public interface IJsonSchemaHelper
{
    /// <summary>
    /// Uses JsonSerializationOptions and JsonSchemaExporterOptions registered as IOptions from DI,
    /// to change behaviour configure them
    /// </summary>
    /// <typeparam name="T">A type for which a JSON scheme is needed</typeparam>
    /// <returns>A JSON scheme</returns>
    string GetJsonScheme<T>();
    
    /// <summary>
    /// Same as GetJsonScheme, but returns BinaryData.
    /// Useful for OpenAI SDK
    /// </summary>
    /// <typeparam name="T">A type for which a JSON scheme is needed</typeparam>
    /// <returns>A JSON scheme in a BinaryData format</returns>
    BinaryData GetBinaryJsonScheme<T>();
}