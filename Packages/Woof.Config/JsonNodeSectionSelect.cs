namespace Woof.Config;

/// <summary>
/// Contains the Select method for the <see cref="JsonNodeSection"/> type.
/// </summary>
public static class JsonNodeSectionSelect {

    /// <summary>
    /// Gets the node by the relative path of the configuration section.
    /// </summary>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The relative path of the configuration section.</param>
    /// <returns>
    /// <see cref="JsonNodeSection"/> matching the <paramref name="path"/>,
    /// or an empty section if the <paramref name="path"/> doesn't match any node.
    /// </returns>
    public static JsonNodeSection Select(this JsonNodeSection section, string path) {
        if (path.Length < 1) return section;
        if (section.Node is null) return JsonNodeSection.Empty;
        var nodePath = new NodePath(path);
        foreach (var part in nodePath.Parts) {
            if (section.NodeType == JsonNodeType.Array && section.Node is JsonArray array) {
                if (!int.TryParse(part.Key, out var index) || index < 0 || index >= array.Count) {
                    section = new JsonNodeSection(JsonNodeType.Empty, part.Path, section);
                    continue;
                }
                var value = array[index];
                section = value is not null
                    ? new JsonNodeSection(value)
                    : new JsonNodeSection(JsonNodeType.Null, part.Path, section);
                if (value is JsonArray or JsonObject) continue;
            }
            else if (section.NodeType == JsonNodeType.Object && section.Node is JsonObject obj) {
                var exists = obj.TryGetPropertyValue(part.Key, out var value);
                section = exists
                    ? (value is not null ? new JsonNodeSection(value) : new JsonNodeSection(JsonNodeType.Null, part.Path, section))
                    : new JsonNodeSection(JsonNodeType.Empty, part.Path, section);
                if (value is JsonArray or JsonObject) continue;
            }
            else if (section.IsNullOrEmpty) {
                section = new JsonNodeSection(JsonNodeType.Empty, part.Path);
            }
        }
        return section;
    }

}