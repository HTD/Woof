namespace Woof.Config;

/// <summary>
/// <see cref="IConfigurationSection"/> bound directly to a <see cref="JsonNode"/> object.
/// </summary>
public class JsonNodeSection : IConfigurationSection {

    /// <summary>
    /// Gets or sets a configuration value.
    /// </summary>
    /// <remarks>
    /// To set value as boolean or number unquoted prefix the value with '='.<br/>
    /// To create a section or array use '{}' and '[]'.
    /// </remarks>
    /// <param name="path">The section path.</param>
    /// <returns>The configuration value.</returns>
    public string? this[string path] {
        get => this.Select(path).Value;
        set => this.Select(path).Value = value;
    }

    /// <summary>
    /// Gets the key this section occupies in its parent.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the full path to this section within the <see cref="IConfiguration"/>.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Contains the node used to create this section.
    /// </summary>
    public JsonNode? Node { get; }

    /// <summary>
    /// Gets the current JSON node as a <see cref="JsonValue"/> node.
    /// </summary>
    public JsonValue ValueNode => Node is JsonValue valueNode ? valueNode : throw new InvalidCastException();

    /// <summary>
    /// Gets the current JSON node as a <see cref="JsonObject"/> node.
    /// </summary>
    public JsonObject ObjectNode => Node is JsonObject objectNode ? objectNode : throw new InvalidCastException();

    /// <summary>
    /// Gets the current JSON node as a <see cref="JsonArray"/> node.
    /// </summary>
    public JsonArray ArrayNode => Node is JsonArray arrayNode ? arrayNode : throw new InvalidCastException();

    /// <summary>
    /// Contains the parent node of the node used to create this section.
    /// </summary>
    public JsonNodeSection? Parent { get; private set; }

    /// <summary>
    /// Gets the type of this node, one of <see cref="JsonNodeType"/> enumeration.
    /// </summary>
    public JsonNodeType NodeType { get; }

    /// <summary>
    /// Gets or sets the section value.
    /// </summary>
    /// <remarks>
    /// Prefix the value with '=' to set the non-string JSON type, unquoted value.
    /// </remarks>
    public string? Value {
        get => Node is JsonValue valueNode ? valueNode.ToString() : null;
        set {
            CreateParentContainer();
            switch (NodeType) {
                case JsonNodeType.Value:
                    var newValueNode = ValueNode.GetValueKind() switch {
                        JsonValueKind.String => JsonValue.Create(value),
                        JsonValueKind.Number => value is null ? null : JsonNodeBinder.GetJsonNumberFromString(value),
                        JsonValueKind.True => JsonValue.Create(value is null || bool.Parse(value)),
                        JsonValueKind.False => JsonValue.Create(value is not null && bool.Parse(value)),
                        _ => null
                    };
                    if (Node?.Parent is JsonObject parentObject) {
                        var propertyName = Key;
                        parentObject.Remove(propertyName);
                        parentObject.Add(propertyName, newValueNode);
                    }
                    else if (Node?.Parent is JsonArray parentArray) {
                        if (int.TryParse(Key, out var index)) {
                            parentArray[index] = newValueNode;
                        }
                    }
                    break;
                case JsonNodeType.Null:
                case JsonNodeType.Empty:
                    if (Parent is null)
                        throw new InvalidOperationException("Cannot set a value when there is no parent node");
                    var node = value switch {
                        "=True" or "=true" => JsonValue.Create(true),
                        "=False" or "=false" => JsonValue.Create(false),
                        "{}" => JsonNode.Parse("{}"),
                        "[]" => JsonNode.Parse("[]"),
                        string s when s.StartsWith('=') => JsonNodeBinder.GetJsonNumberFromString(s[1..]),
                        _ => JsonValue.Create(value)
                    };
                    if (Parent?.Node is JsonObject obj) {
                        try { obj.Remove(Key); } catch { }
                        obj.Add(Key, node);
                    }
                    else if (Parent?.Node is JsonArray array) array.Add(node);
                    break;
                default: throw new InvalidOperationException($"Cannot set a value on {NodeType} node");
            }
        }
    }

    /// <summary>
    /// Gets a value indicating the section either contains a null value or is empty.
    /// </summary>
    public bool IsNullOrEmpty => Node is null;

    /// <summary>
    /// Gets an empty <see cref="IConfigurationSection"/> value.
    /// </summary>
    public static JsonNodeSection Empty { get; } = new(JsonNodeType.Empty, "");

    /// <summary>
    /// Creates a new <see cref="IConfigurationSection"/> directly from <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="node">A JSON token to create the configuration section.</param>
    public JsonNodeSection(JsonNode node) {
        NodeType = node switch {
            JsonObject => JsonNodeType.Object,
            JsonValue => JsonNodeType.Value,
            JsonArray => JsonNodeType.Array,
            _ => throw new NotSupportedException("Unsupported node type")
        };
        Node = node;
        Parent = node is null || node.Parent is null ? null : new JsonNodeSection(node.Parent);
        Path = Node is null ? String.Empty : NodePath.GetSectionPath(Node.GetPath());
        var lastSeparatorIndex = Path.LastIndexOf(':');
        Key = Path[(lastSeparatorIndex + 1)..];
    }

    /// <summary>
    /// Creates <see cref="JsonNodeSection"/> from metadata.
    /// </summary>
    /// <param name="nodeType">The type of the node.</param>
    /// <param name="path">Section path.</param>
    /// <param name="parent">Parent section if available.</param>
    internal JsonNodeSection(JsonNodeType nodeType, string path = "", JsonNodeSection? parent = null) {
        NodeType = nodeType;
        Path = path;
        Parent = parent;
        var lastSeparatorIndex = path.LastIndexOf(':');
        Key = path[(lastSeparatorIndex + 1)..];
    }

    /// <summary>
    /// Creates a <see cref="JsonNodeSection"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="JsonNodeSection"/> instance.</returns>
    public static JsonNodeSection Parse(string json, bool caseSensitive = false) => new JsonNodeLoader().Parse(json, caseSensitive);

    /// <summary>
    /// Gets the immediate descendant configuration sub-sections.
    /// </summary>
    /// <returns>The configuration sub-sections.</returns>
    public IEnumerable<IConfigurationSection> GetChildren()
        => Node switch {
            JsonObject objectNode => objectNode.Select(i => new JsonNodeSection(i.Value!)),
            JsonArray arrayNode => arrayNode.Select(i => new JsonNodeSection(i!)),
            _ => Enumerable.Empty<IConfigurationSection>()
        };

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Exception.</returns>
    /// <exception cref="NotImplementedException">Invoked.</exception>
    public IChangeToken GetReloadToken() => throw new NotImplementedException("No automatic reloading here");

    /// <summary>
    /// Gets a configuration sub-section with the specified key.
    /// </summary>
    /// <param name="key">The key of the configuration section.</param>
    /// <returns>The Microsoft.Extensions.Configuration.IConfigurationSection.</returns>
    /// <remarks>
    /// This method will never return null. If no matching sub-section is found with
    /// the specified key, an empty Microsoft.Extensions.Configuration.IConfigurationSection
    /// will be returned.
    /// </remarks>
    public IConfigurationSection GetSection(string key) => this.Select(key);

    /// <summary>
    /// Gets the configuration section as the JSON string.
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => Node?.ToJsonString();

    /// <summary>
    /// Creates a parent container for this node.
    /// Throws if the grandparent of this node is not an object node.
    /// </summary>
    /// <exception cref="InvalidOperationException">Grandparent node is not an object node.</exception>
    private void CreateParentContainer() {
        if (Parent is not null) return;
        var nPath = new NodePath(Path);
        var parentPart = nPath.Parent;
        var parentSection = this.Select(parentPart.Path);
        var grandparentSection = parentSection.Parent;
        if (grandparentSection?.NodeType != JsonNodeType.Object)
            throw new InvalidOperationException("Target path too far from the ancestor");
        grandparentSection[parentPart.Key] = int.TryParse(nPath.Key, out _) ? "[]" : "{}";
    }

}