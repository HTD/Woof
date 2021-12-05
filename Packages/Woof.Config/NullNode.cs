namespace Woof.Config;

/// <summary>
/// Contains a null value node information.
/// </summary>
public class NullNode  {

    /// <summary>
    /// Gets the parent node of the null node.
    /// </summary>
    public JsonNode? Parent { get; }

    /// <summary>
    /// Gets the property name that is set to a null value.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Creates the special null node metadata.
    /// </summary>
    /// <param name="parent">Parent node containing the property with a null value.</param>
    /// <param name="propertyName">The name of the property that is set to a null value.</param>
    public NullNode(JsonNode? parent, string propertyName) {
        Parent = parent;
        PropertyName = propertyName;
    }

}