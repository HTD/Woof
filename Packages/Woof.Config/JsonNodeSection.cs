namespace Woof.Config;

/// <summary>
/// <see cref="IConfigurationSection"/> bound directly to a <see cref="JsonNode"/> object.
/// </summary>
public class JsonNodeSection : IConfigurationSection {

    /// <summary>
    /// Gets or sets a configuration value.
    /// </summary>
    /// <remarks>
    /// To set value as boolean or number prefix the value with '=', to set object and array use '{}' and '[]'.
    /// </remarks>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value.</returns>
    public string? this[string key] {
        get => GetNodeSection(key).Value;
        set => GetNodeSection(key, nullNodeIfNotExists: true).Value = value;
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
    /// Gets or sets the section value.
    /// </summary>
    /// <exception cref="InvalidCastException">Incompatible section / token type.</exception>
    /// <exception cref="InvalidDataException">Invalid string value for the token data type.</exception>
    public string? Value {
        get => Node is JsonValue valueNode ? valueNode.ToString() : null;
        set {
            if (IsNull) {
                JsonValueKind kind = value switch {
                    "=true" => JsonValueKind.True,
                    "=false" => JsonValueKind.False,
                    "=null" => JsonValueKind.Null,
                    "{}" => JsonValueKind.Object,
                    "[]" => JsonValueKind.Array,
                    string s when s.StartsWith('=') => JsonValueKind.Number,
                    _ => JsonValueKind.String
                };
                var node = kind switch {
                    JsonValueKind.String => JsonValue.Create(value),
                    JsonValueKind.Null => null,
                    JsonValueKind.True => JsonValue.Create(true),
                    JsonValueKind.False => JsonValue.Create(false),
                    JsonValueKind.Object => JsonObject.Parse("{}"),
                    JsonValueKind.Array => JsonArray.Parse("[]"),
                    JsonValueKind.Number => GetJsonNumberFromString(value!),
                    _ => null
                };
                //var root = (JsonObject.Parse("{}") as JsonObject)!;
                if (Parent is JsonObject o) {
                    try { o.Remove(Key); } catch { }
                    o.Add(Key, node);
                }
                else if (Parent is JsonArray a) a.Add(node);
                return;
            }
            if (Node is not JsonValue valueNode) return;
            var replacement = valueNode.GetValueKind() switch {
                JsonValueKind.String => JsonValue.Create(value),
                JsonValueKind.Number => value is null ? null : GetJsonNumberFromString(value),
                JsonValueKind.True => JsonValue.Create(value is null || bool.Parse(value)),
                JsonValueKind.False => JsonValue.Create(value is not null && bool.Parse(value)),
                _ => null
            };
            if (Node?.Parent is JsonObject parentObject) {
                var propertyName = Key;
                parentObject.Remove(propertyName);
                parentObject.Add(propertyName, replacement);
            }
        }
    }

    /// <summary>
    /// Gets a JSON value from string.
    /// </summary>
    /// <param name="json">Valid JSON string.</param>
    /// <returns>A <see cref="JsonValue"/> instance.</returns>
    private JsonValue GetJsonNumberFromString(string json) {
        if (json.StartsWith('=')) json = json[1..];
        return json.Contains('.')
            ? JsonValue.Create(double.Parse(json, CultureInfo.InvariantCulture))
            : JsonValue.Create(long.Parse(json, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Gets an empty <see cref="IConfigurationSection"/> value.
    /// </summary>
    public static JsonNodeSection Empty { get; } = new();

    /// <summary>
    /// Gets or sets the default loader for the section.
    /// </summary>
    public IJsonNodeLoader Loader {
        get => _Loader ?? new JsonNodeLoader();
        set => _Loader = value;
    }

    /// <summary>
    /// Gets or sets the defult property binder.
    /// </summary>
    public IPropertyBinder Binder {
        get => _Binder ?? new PropertyBinder();
        set => _Binder = value;
    }

    /// <summary>
    /// Gets a value indicating the section is empty and the node for it does not exist.
    /// </summary>
    public bool IsEmpty => Key.Length < 1;

    /// <summary>
    /// Gets a value indicating the section contains a null value, but is not empty, because it has a key.
    /// </summary>
    public bool IsNull => Node is null && Key.Length > 0;

    /// <summary>
    /// Gets a value indicating the section either contains a null value or is empty.
    /// </summary>
    public bool IsNullOrEmpty => Node is null;

    /// <summary>
    /// Creates a new <see cref="IConfigurationSection"/> directly from <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="node">A JSON token to create the configuration section.</param>
    public JsonNodeSection(JsonNode node) {
        Node = node;
        Parent = node?.Parent;
        Key = Node?.GetPath().Split('.').LastOrDefault()?.TrimStart('$', '.') ?? string.Empty;
        Path =  Node is null ? String.Empty : GetKeyPath(Node.GetPath().TrimStart('$', '.'));
    }

    /// <summary>
    /// Creates a <see cref="JsonNodeSection"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="JsonNodeSection"/> instance.</returns>
    public static JsonNodeSection Parse(string json, bool caseSensitive = false) => new JsonNodeLoader().Parse(json, caseSensitive);

    /// <summary>
    /// Binds this configuration to <typeparamref name="T"/> object properties and returns it.
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    /// <returns>Configuration.</returns>
    public T Get<T>() where T : class, new() => Binder.Get<T>(this);

    /// <summary>
    /// Updates this configuration from <typeparamref name="T"/> object properties.
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    /// <param name="value">Configuration.</param>
    public void Set<T>(T value) where T : class, new() => Binder.Set<T>(this, value);

    /// <summary>
    /// Creates an empty <see cref="IConfigurationSection"/>, the node for this section does not exist.
    /// </summary>
    private JsonNodeSection() {
        Key = string.Empty;
        Path = string.Empty;
    }

    /// <summary>
    /// Create a new <see cref="IConfiguration"/> null node, a node that has a path and parent, but not the value.
    /// </summary>
    /// <param name="node">An instance of <see cref="NullNode"/>.</param>
    private JsonNodeSection(NullNode node) {
        Key = node.PropertyName;
        Path = node.Parent?.GetPath() + '.' + node.PropertyName ?? "";
        Parent = node.Parent;
    }

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
    public IChangeToken GetReloadToken() => throw new NotImplementedException();

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
    public IConfigurationSection GetSection(string key) => GetNodeSection(key);

    /// <summary>
    /// Gets the configuration section as the JSON string.
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => Node?.ToJsonString();

    /// <summary>
    /// Gets the configuration section path by the node path.
    /// </summary>
    /// <param name="nodePath">Node path.</param>
    /// <returns>The key of the configuration section.</returns>
    public static string GetKeyPath(string nodePath)
        => RxNodePathIndex.Replace(nodePath, ":$1").Replace('.', ':') is string s ? s.StartsWith('$') ? s[2..] : s : string.Empty;

    /// <summary>
    /// Gets the node path by configuration section path.
    /// </summary>
    /// <param name="keyPath">The key of the configuration section.</param>
    /// <returns>Node path.</returns>
    public static string GetNodePath(string keyPath) => RxIConfigurationSectionIndex.Replace(keyPath, "[$1]").Replace(':', '.');

    /// <summary>
    /// Gets the node by the key of the configuration section.
    /// </summary>
    /// <param name="key">The key of the configuration section.</param>
    /// <param name="nullNodeIfNotExists">Returns a null node if the property doesn't exist.</param>
    /// <returns>Token matched or null.</returns>
    private JsonNodeSection GetNodeSection(string key, bool nullNodeIfNotExists = false) {
        var node = Node;
        foreach (var part in key.Split(':')) {
            if (node is JsonArray array) {
                node = int.TryParse(part, out var index) && index >= 0 && index < array.Count ? array[index] : null;
                if (node is JsonArray or JsonObject) continue;
                else return node is null ? nullNodeIfNotExists ? new JsonNodeSection(new NullNode(Node, part)) : Empty : new JsonNodeSection(node);
            }
            if (node is JsonObject obj) {
                var exists = obj.TryGetPropertyValue(part, out var value);
                if (value is null) return nullNodeIfNotExists || exists ? new JsonNodeSection(new NullNode(Node, part)) : Empty;
                node = value;
            }
            if (node is null) return Empty;
        }
        return node is null ? Empty : new JsonNodeSection(node);
    }

    /// <summary>
    /// Contains the node used to create this section.
    /// </summary>
    public JsonNode? Node;

    /// <summary>
    /// Contains the parent node of the node used to create this section.
    /// </summary>
    public readonly JsonNode? Parent;

    /// <summary>
    /// <see cref="Loader"/> backing field.
    /// </summary>
    private IJsonNodeLoader? _Loader;

    /// <summary>
    /// <see cref="Binder"/> backing field.
    /// </summary>
    private IPropertyBinder? _Binder;

    /// <summary>
    /// Matches the <see cref="JsonNode"/> indices.
    /// </summary>
    private static readonly Regex RxNodePathIndex = new(@"\[(\d+)\]", RegexOptions.Compiled);

    /// <summary>
    /// Matches the <see cref="IConfiguration"/> indices.
    /// </summary>
    private static readonly Regex RxIConfigurationSectionIndex = new(@":(\d+)", RegexOptions.Compiled);

}