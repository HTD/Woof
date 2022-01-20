namespace Woof.Settings.Analysis;

/// <summary>
/// A class enumerating the <see cref="JsonNode"/> tree.
/// </summary>
public class JsonNodeTree {

    /// <summary>
    /// Gets the target node.
    /// </summary>
    public JsonNode Target { get; }

    /// <summary>
    /// Creates a property tree metadata over a node.
    /// </summary>
    /// <param name="target">Target node.</param>
    public JsonNodeTree(JsonNode target) => Target = target;

    /// <summary>
    /// Ensure that the parent node for the node exists.
    /// </summary>
    /// <param name="node">Target node.</param>
    /// <param name="rootContext">Root context.</param>
    public void EnsureParentExists(ObjectTreeNode node, object rootContext) {
        if (node.Path.Length < 2) return; // root and direct properties always exist
        foreach (var part in node.Path.Parent!.Parts.Skip(1)) {
            var target = Target.Root.Select(part);
            var value = ObjectTree.GetPropertyByPath(rootContext, part).Value;
            if (target is null && value is not null)
                Target.Root.SetNode(part, JsonNode.Parse(value is ICollection ? "[]" : "{}")!);
        }
    }

    /// <summary>
    /// Sets the property from <see cref="ObjectTreeNode"/>.
    /// </summary>
    /// <param name="node">Property node.</param>
    /// <param name="rootContext">Root context.</param>
    public void SetProperty(ObjectTreeNode node, object rootContext) {
        var special = node.Info?.GetCustomAttribute<SpecialAttribute>();
        if (special is not null) return;
        if (node.Content is null) return;
        EnsureParentExists(node, rootContext);
        Target.Root.SetValue(node.Path, node.Content);
    }

    /// <summary>
    /// Enumerates all the <see cref="JsonNode"/> tree leaves.
    /// </summary>
    /// <param name="root">Root node.</param>
    /// <returns>Enumeration of all <see cref="JsonNode"/> leaves in BFS order.</returns>
    public static IEnumerable<JsonNodeTreeNode> Traverse(JsonNode root) {
        Queue<JsonNodeTreeNode> q = new();
        JsonNodeTreeNode node;
        q.Enqueue(new JsonNodeTreeNode(root));
        while (q.Count > 0) {
            node = q.Dequeue();
            if (node.Content is JsonValue or null) yield return node;
            if (node.Content is JsonObject jsonObject)
                foreach (var property in jsonObject)
                    q.Enqueue(new JsonNodeTreeNode(node.Path, property.Key, property.Value));
            else if (node.Content is JsonArray jsonArray)
                for (int i = 0, n = jsonArray.Count; i < n; i++)
                    q.Enqueue(new JsonNodeTreeNode(node.Path, i.ToString(), jsonArray[i]));
        }
    }

}
