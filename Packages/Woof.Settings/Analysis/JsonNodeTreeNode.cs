namespace Woof.Settings.Analysis;

/// <summary>
/// Contains the value with the path within the node tree.
/// </summary>
public struct JsonNodeTreeNode {

    /// <summary>
    /// Contains the path to the node.
    /// </summary>
    public JsonNodePath Path;

    /// <summary>
    /// Contains the node.
    /// </summary>
    public JsonNode? Content;

    /// <summary>
    /// Gets the node as <see cref="JsonValue"/>.
    /// </summary>
    public readonly JsonValue? Value => Content as JsonValue;

    /// <summary>
    /// Gets the root node of the content node.
    /// </summary>
    public readonly JsonNode? Root => Content?.Root;

    /// <summary>
    /// Creates a tree node.
    /// </summary>
    /// <param name="node">The node.</param>
    public JsonNodeTreeNode(JsonNode node) {
        Path = new JsonNodePath(node.GetPath());
        Content = node;
    }

    /// <summary>
    /// Creates a tree node with a specified path, an added key and a value.
    /// </summary>
    /// <param name="path">Path to the parent node.</param>
    /// <param name="key">The key of the node.</param>
    /// <param name="node">The node.</param>
    public JsonNodeTreeNode(JsonNodePath path, string key, JsonNode? node) {
        Path = new JsonNodePath(path, key);
        Content = node;
    }

    /// <summary>
    /// Returns the leaf path and value as string.
    /// </summary>
    /// <returns>Leaf path.</returns>
    public override readonly string ToString() => $"{Path} => {Content}";

}