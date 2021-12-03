namespace Woof.Config.Internals;

/// <summary>
/// Contains extension methods for <see cref="JsonNode"/>.
/// </summary>
public static class JsonNodeTraits {

    /// <summary>
    /// Gets the <see cref="JsonValue"/> as a <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="valueNode">A value node.</param>
    /// <returns><see cref="JsonElement"/>.</returns>
    public static JsonElement AsJsonElement(this JsonValue valueNode) => (JsonElement)ValueProperty.GetValue(valueNode)!;

    /// <summary>
    /// Gets the <see cref="JsonValueKind"/> hidden property of a <see cref="JsonValue"/> node.
    /// </summary>
    /// <param name="valueNode">A value node.</param>
    /// <returns><see cref="JsonValueKind"/>.</returns>
    public static JsonValueKind GetValueKind(this JsonValue valueNode) => valueNode.AsJsonElement().ValueKind;

    /// <summary>
    /// Gets the <see cref="JsonValueKind"/> hidden property of a <see cref="JsonNode"/> node, if it is a <see cref="JsonValue"/>.
    /// </summary>
    /// <param name="node">A node.</param>
    /// <returns><see cref="JsonValueKind"/> or null.</returns>
    public static JsonValueKind? GetValueKind(this JsonNode node)
        => node is JsonValue valueNode ? valueNode.AsJsonElement().ValueKind : null;

    /// <summary>
    /// Hidden "Value" property of the <see cref="JsonValue"/> underlying type.
    /// </summary>
    private static readonly PropertyInfo ValueProperty = JsonValue.Parse("0")!.GetType().GetProperty("Value")!;

}