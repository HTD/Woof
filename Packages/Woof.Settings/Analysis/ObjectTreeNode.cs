namespace Woof.Settings.Analysis;

/// <summary>
/// Contains a property with the path from its root.
/// </summary>
public readonly struct ObjectTreeNode {

    /// <summary>
    /// The path to the node.
    /// </summary>
    public readonly JsonNodePath Path;

    /// <summary>
    /// The type of the node.
    /// </summary>
    public readonly Type? Type;

    /// <summary>
    /// The content of the node.
    /// </summary>
    public readonly object? Content;

    /// <summary>
    /// Property info of the node.
    /// </summary>
    public readonly PropertyInfo? Info;

    /// <summary>
    /// Creates a new direct property.
    /// </summary>
    /// <param name="owner">Property owner.</param>
    /// <param name="property">Property.</param>
    public ObjectTreeNode(object owner, PropertyInfo property) {
        Info = property;
        Path = new JsonNodePath("$", property.Name);
        Type = property.PropertyType;
        Content = property.GetValue(owner);
    }

    /// <summary>
    /// Creates a collection item node from <see cref="ICollection"/> node and index.
    /// </summary>
    /// <param name="node"><see cref="ICollection"/> node.</param>
    /// <param name="index">Element index.</param>
    /// <exception cref="InvalidCastException">The content is not a collection.</exception>
    public ObjectTreeNode(ObjectTreeNode node, int index) {
        Info = null;
        Path = new JsonNodePath(node.Path, index.ToString());
        Content = node.Content switch {
            Array arrayContent => arrayContent.GetValue(index),
            IList listContent => listContent[index],
            _ => throw new InvalidCastException()
        };
        Type = Content?.GetType();
    }

    /// <summary>
    /// Creates a child node from parent node and its property.
    /// </summary>
    /// <param name="node">Parent node.</param>
    /// <param name="property">Child property.</param>
    public ObjectTreeNode(ObjectTreeNode node, PropertyInfo property) {
        Info = property;
        Path = new JsonNodePath(node.Path, property.Name);
        Content = node.Content is null ? null : property.GetValue(node.Content);
        Type = Content?.GetType();
    }

    /// <summary>
    /// Returns the leaf path and value as string.
    /// </summary>
    /// <returns>Leaf path.</returns>
    public override readonly string ToString() => $"{Path} => {Content}";

}