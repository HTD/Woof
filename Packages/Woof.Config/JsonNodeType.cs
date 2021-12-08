namespace Woof.Config;

/// <summary>
/// JSON node types.
/// </summary>
public enum JsonNodeType {

    /// <summary>
    /// Empty node, does not exist in JSON document.
    /// </summary>
    Empty,

    /// <summary>
    /// Null node, exists in JSON document, but does not have a value.
    /// </summary>
    Null,

    /// <summary>
    /// Object node, contains properties.
    /// </summary>
    Object,

    /// <summary>
    /// Array node, contains elements.
    /// </summary>
    Array,

    /// <summary>
    /// Value node, contains a value.
    /// </summary>
    Value

}