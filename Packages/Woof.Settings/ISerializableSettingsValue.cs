namespace Woof.Settings;

/// <summary>
/// Implementing type can be serialized from object to string and deserialized form string to object.
/// </summary>
/// <remarks>
/// The implementing type is automatically converted by <see cref="ValueConversions"/> module.
/// </remarks>
public interface ISerializableSettingsValue {

    /// <summary>
    /// Gets the serialized value.
    /// </summary>
    /// <returns>Serialized string.</returns>
    string GetString();

    /// <summary>
    /// Parses the string into the implemented instance.
    /// </summary>
    /// <param name="value">Serialized string.</param>
    void Parse(string value);


}