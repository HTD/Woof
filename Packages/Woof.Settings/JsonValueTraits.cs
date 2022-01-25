namespace Woof.Settings;

/// <summary>
/// Contains the extension methods for the <see cref="JsonValue"/> type.
/// </summary>
public static class JsonValueTraits {

    /// <summary>
    /// Gets the <see cref="JsonValue"/> as a <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="valueNode">A value node.</param>
    /// <returns><see cref="JsonElement"/>.</returns>
    public static JsonElement AsJsonElement(this JsonValue valueNode) => (JsonElement)ValueProperty.GetValue(valueNode)!;

    /// <summary>
    /// Gets the <see cref="JsonValueKind"/> hidden property value of a <see cref="JsonValue"/> node.
    /// </summary>
    /// <param name="valueNode">A value node.</param>
    /// <returns><see cref="JsonValueKind"/>.</returns>
    public static JsonValueKind GetValueKind(this JsonValue valueNode) => valueNode.AsJsonElement().ValueKind;

    /// <summary>
    /// Gets another value of the same kind from a string.
    /// </summary>
    /// <param name="valueNode">Target value node.</param>
    /// <param name="value">A string representing a new value.</param>
    /// <returns>A <see cref="JsonValue"/> instance.</returns>
    public static JsonValue? GetAnotherFromString(this JsonValue valueNode, string? value)
        => valueNode.GetValueKind() switch {
            JsonValueKind.String => JsonValue.Create(value),
            JsonValueKind.Number => value is null ? null : GetJsonNumberFromString(value),
            JsonValueKind.True => JsonValue.Create(value is null || bool.Parse(value)),
            JsonValueKind.False => JsonValue.Create(value is not null && bool.Parse(value)),
            _ => null
        };

    /// <summary>
    /// Gets a JSON numeric value from a string.
    /// </summary>
    /// <param name="json">A valid JSON numeric litaral.</param>
    /// <returns>A <see cref="JsonValue"/> instance.</returns>
    public static JsonValue GetJsonNumberFromString(string json)
        => json.Contains('.')
            ? JsonValue.Create(double.Parse(json, N))
            : json[0] == '-' ? JsonValue.Create(long.Parse(json, N)) : JsonValue.Create(ulong.Parse(json, N));

    /// <summary>
    /// Tries to get a <paramref name="value"/> of the specified <paramref name="type"/> from a string.
    /// </summary>
    /// <param name="valueNode">Value node.</param>
    /// <param name="type">Target type.</param>
    /// <param name="value">Converted value.</param>
    /// <returns>True if the <paramref name="value"/> was converted using supported value conversions.</returns>
    public static bool TryConvert(this JsonValue valueNode, Type type, out object? value) {
        if (type.BaseType == typeof(Enum)) {
            value = Enum.Parse(type, valueNode.ToString());
            return true;
        }
        try {
            value = ValueConversions.Parse(valueNode.ToString(), type);
            return true;
        }
        catch {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get a <paramref name="value"/> from a value node.
    /// </summary>
    /// <param name="valueNode">Value node.</param>
    /// <param name="value">Converted value or default <typeparamref name="T"/>.</param>
    /// <returns>True if the <paramref name="value"/> was converted using supported value conversions.</returns>
    public static bool TryConvert<T>(this JsonValue valueNode, out T? value) {
        var type = typeof(T);
        if (type.BaseType == typeof(Enum)) {
            value = (T)Enum.Parse(type, valueNode.ToString());
            return true;
        }
        try {
            value = (T)ValueConversions.Parse(valueNode.ToString(), type);
            return true;
        }
        catch {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Converts the <see cref="JsonValue"/> to <typeparamref name="T"/> using supported value conversions.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="valueNode">Value node.</param>
    /// <returns>Converted value or default <typeparamref name="T"/>.</returns>
    public static T? As<T>(this JsonValue valueNode) {
        var type = typeof(T);
        if (type.BaseType == typeof(Enum)) return (T)Enum.Parse(type, valueNode.ToString());
        try {
            return (T)ValueConversions.Parse(valueNode.ToString(), type);
        }
        catch {
            return default;
        }
    }

    /// <summary>
    /// Hidden "Value" property of the <see cref="JsonValue"/> underlying type.
    /// </summary>
    private static readonly PropertyInfo ValueProperty = JsonNode.Parse("0")!.GetType().GetProperty("Value")!;

    /// <summary>
    /// Invariant culture format provider for JSON compatible conversions.
    /// </summary>
    private static readonly IFormatProvider N = CultureInfo.InvariantCulture;

}