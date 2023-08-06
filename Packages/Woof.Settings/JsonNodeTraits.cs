namespace Woof.Settings;

/// <summary>
/// Contains extension methods for the <see cref="JsonNode"/> type.
/// </summary>
public static class JsonNodeTraits {

    /// <summary>
    /// Gets a value at the specified path.
    /// </summary>
    /// <param name="root">Root node.</param>
    /// <param name="path">Path to get a value from.</param>
    /// <returns>A <see cref="JsonValue"/> or null.</returns>
    public static JsonValue? GetValueNode(this JsonNode root, JsonNodePath path) => root.Select(path)?.AsValue();

    /// <summary>
    /// Gets the <see cref="JsonValueKind"/> hidden property value of a <see cref="JsonNode"/> node, if it is a <see cref="JsonValue"/>.
    /// </summary>
    /// <param name="node">A JSON node.</param>
    /// <returns><see cref="JsonValueKind"/> or null.</returns>
    public static JsonValueKind? GetValueKind(this JsonNode node) => node is JsonValue valueNode ? valueNode.AsJsonElement().ValueKind : null;

    /// <summary>
    /// Sets a <paramref name="node"/> at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="root">Local root node.</param>
    /// <param name="path">Path to place the node at.</param>
    /// <param name="node">The node to set.</param>
    /// <exception cref="InvalidOperationException">Parent node doesn't exist.</exception>
    public static void SetNode(this JsonNode root, JsonNodePath path, JsonNode node) {
        JsonNode? parent = (path.Parent is JsonNodePath parentPath ? root.Select(parentPath) : root) ?? throw new InvalidOperationException("The parent node doesn't exist");
        if (parent is JsonObject parentObject) {
            if (parentObject.TryGetPropertyValue(path.Key, out _)) parentObject.Remove(path.Key);
            parentObject.Add(path.Key, node);
        }
        else if (parent is JsonArray jsonArray) {
            var index = path.Index;
            if (jsonArray.Count > index) jsonArray.RemoveAt(index);
            jsonArray.Insert(index, node);
        }
    }

    /// <summary>
    /// Sets a value at the specified path.
    /// </summary>
    /// <param name="root">Root node.</param>
    /// <param name="path">Path to set the value at.</param>
    /// <param name="value">Value node to set.</param>
    public static void SetValueNode(this JsonNode root, JsonNodePath path, JsonValue? value = null) {
        JsonNode? parent = path.Parent is JsonNodePath parentPath ? root.Select(parentPath) : root;
        if (parent is null) {
            // knowing the keys we can create the parent node if it doesn't exist:
            var grandparent = (path.Length < 3 ? root : root.Select(path.Parts.Select(p => p.ToString()).ElementAt(^3))) ?? throw new InvalidOperationException("Target path too far from an existing ancestor");
            var parentKey = path.Parent!.Key;
            var targetKey = path.Key;
            var parentNode = JsonNode.Parse(int.TryParse(targetKey, out _) ? "[]" : "{}");
            var grandparentObject = grandparent.AsObject();
            if (grandparentObject.TryGetPropertyValue(parentKey, out _)) grandparentObject.Remove(parentKey);
            grandparentObject.Add(parentKey, parentNode);
            parent = grandparent[parentKey]!;
        }
        if (int.TryParse(path.Key, out int index)) parent.AsArray()[index] = value;
        else {
            var parentObject = parent.AsObject();
            if (parentObject.TryGetPropertyValue(path.Key, out var current)) {
                if (current?.ToString() == value?.ToString()) return;
                parentObject.Remove(path.Key);
            }
            parentObject.Add(path.Key, value);
        }
    }

    /// <summary>
    /// Selects a <see cref="JsonNode"/> by path.
    /// </summary>
    /// <param name="node">This node.</param>
    /// <param name="path">Path to select.</param>
    /// <returns><see cref="JsonNode"/> found or null.</returns>
    public static JsonNode? Select(this JsonNode node, JsonNodePath path) {
        JsonNode? current = node;
        if (string.IsNullOrEmpty(path)) return node;
        foreach (var key in path.Keys) {
            if (key == "$") continue;
            if (current is null) return null;
            if (int.TryParse(key, out int index) && current is JsonArray jsonArray) {
                current = index < jsonArray.Count ? current[index] : null;
            }
            else if (!current.AsObject().TryGetPropertyValue(key, out current)) return null;
        }
        return current;
    }

    /// <summary>
    /// Selects <see cref="JsonNode"/> by path.
    /// </summary>
    /// <param name="node">This node.</param>
    /// <param name="path">Path to select.</param>
    /// <param name="result"><see cref="JsonNode"/> found or null.</param>
    /// <returns>True if the node is found.</returns>
    public static bool TrySelect(this JsonNode node, string path, out JsonNode? result) {
        JsonNode? current = node;
        foreach (var key in JsonNodePath.Split(path)) {
            if (current is null) { result = null; return false; }
            if (int.TryParse(key, out int index)) current = current[index];
            else if (!current.AsObject().TryGetPropertyValue(key, out current)) { result = null; return false; }
        }
        result = current;
        return true;
    }

}