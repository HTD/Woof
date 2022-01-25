namespace Woof.Settings;

/// <summary>
/// Builds the new <see cref="JsonValue"/> instances using <see cref="ValueConversions"/>.
/// </summary>
public class JsonValueBuilder {

    /// <summary>
    /// Tries to get a <see cref="JsonValue"/> from a <paramref name="value"/> of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">A type to convert from.</param>
    /// <param name="value">A value to convert from.</param>
    /// <param name="valueNode">Value node.</param>
    /// <returns>True if the conversion was successful.</returns>
    public static bool TryConvert(Type type, object? value, out JsonValue? valueNode) {
        if (value is null) {
            valueNode = null;
            return true;
        }
        if (type.BaseType == typeof(Enum)) {
            valueNode = JsonValue.Create<string>(value.ToString());
            return true;
        }
        try {
            var valueString = ValueConversions.GetString(value, out var isQuoted);
            valueNode = isQuoted ? JsonValue.Create(valueString) : valueString switch {
                "True" or "true" => JsonValue.Create(true),
                "False" or "false" => JsonValue.Create(false),
                _ => JsonValueTraits.GetJsonNumberFromString(valueString),
            };
            return true;
        }
        catch {
            valueNode = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get a <see cref="JsonValue"/> from a <paramref name="value"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to convert from.</typeparam>
    /// <param name="value">A value to convert from.</param>
    /// <param name="valueNode">Value node.</param>
    /// <returns>True if the conversion was successful.</returns>
    public static bool TryConvert<T>(T? value, out JsonValue? valueNode) {
        var type = typeof(T);
        if (value is null) {
            valueNode = default;
            return true;
        }
        if (type.BaseType == typeof(Enum)) {
            valueNode = JsonValue.Create<string>(value.ToString());
            return true;
        }
        try {
            var valueString = ValueConversions.GetString(value, out var isQuoted);
            valueNode = isQuoted ? JsonValue.Create(valueString) : valueString switch {
                "True" or "true" => JsonValue.Create(true),
                "False" or "false" => JsonValue.Create(false),
                _ => JsonValueTraits.GetJsonNumberFromString(valueString),
            };
            return true;
        }
        catch {
            valueNode = default;
            return false;
        }
    }

}