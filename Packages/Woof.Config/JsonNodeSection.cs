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
        set {
            var lastSeparatorOffset = key.LastIndexOf(':');
            if (lastSeparatorOffset < 0) GetNodeSection(key, nullNodeIfNotExists: true).Value = value;
            else
                GetNodeSection(key[0..lastSeparatorOffset])
                    .GetNodeSection(key[0..(lastSeparatorOffset + 1)], nullNodeIfNotExists: true).Value = value;
        }
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
    /// <remarks>
    /// Prefix the value with '=' to set the non-string JSON type, unquoted value.
    /// </remarks>
    public string? Value {
        get => Node is JsonValue valueNode ? valueNode.ToString() : null;
        set {
            if (IsNull) {
                var node = value switch {
                    "=True" or "=true" => JsonValue.Create(true),
                    "=False" or "=false" => JsonValue.Create(false),
                    "=Null" or "=null" => null,
                    "{}" => JsonObject.Parse("{}"),
                    "[]" => JsonArray.Parse("[]"),
                    string s when s.StartsWith('=') => GetJsonNumberFromString(s[1..]),
                    _ => JsonValue.Create(value)
                };
                if (Parent is JsonObject o) {
                    try { o.Remove(Key); } catch { }
                    o.Add(Key, node);
                }
                else if (Parent is JsonArray a) a.Add(node);
                return;
            }
            if (Node is not JsonValue valueNode) return;
            if (Node?.Parent is JsonObject parentObject) {
                var propertyName = Key;
                parentObject.Remove(propertyName);
                parentObject.Add(propertyName, valueNode.GetValueKind() switch {
                    JsonValueKind.String => JsonValue.Create(value),
                    JsonValueKind.Number => value is null ? null : GetJsonNumberFromString(value),
                    JsonValueKind.True => JsonValue.Create(value is null || bool.Parse(value)),
                    JsonValueKind.False => JsonValue.Create(value is not null && bool.Parse(value)),
                    _ => null
                });
            }
        }
    }

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
    /// Gets an empty <see cref="IConfigurationSection"/> value.
    /// </summary>
    public static JsonNodeSection Empty { get; } = new();

    /// <summary>
    /// Creates a new <see cref="IConfigurationSection"/> directly from <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="node">A JSON token to create the configuration section.</param>
    public JsonNodeSection(JsonNode node) {
        Node = node;
        Parent = node?.Parent;
        Path =  Node is null ? String.Empty : NodePath.GetSectionPath(Node.GetPath());
        var lastSeparatorIndex = Path.LastIndexOf(':');
        Key = Path[(lastSeparatorIndex + 1)..];
    }

    /// <summary>
    /// Creates a <see cref="JsonNodeSection"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="JsonNodeSection"/> instance.</returns>
    public static JsonNodeSection Parse(string json, bool caseSensitive = false) => new JsonNodeLoader().Parse(json, caseSensitive);

    /// <summary>
    /// Get a <typeparamref name="T"/> object built from this configuration.
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    /// <returns>Configuration.</returns>
    public T Get<T>() where T : class, new() => Binder.Get<T>(this);

    /// <summary>
    /// Gets a value of type <typeparamref name="T"/> by the section path.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="path">Section path.</param>
    /// <returns>Configuration value or default <typeparamref name="T"/>.</returns>
    public T GetValue<T>(string path)
        => PropertyBinder.TryGetValue(typeof(T), GetSection(path).Value, out var value) && value is not null ? (T)value : default!;

    /// <summary>
    /// Gets a value of type <typeparamref name="T"/> by the section path.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="path">Section path.</param>
    /// <param name="fallback">Default value in case the section doesn't exist or the conversion fails.</param>
    /// <returns>Configuration or <paramref name="fallback"/> value.</returns>
    public T GetValue<T>(string path, T fallback)
        => PropertyBinder.TryGetValue(typeof(T), GetSection(path).Value, out var value) && value is not null ? (T)value : fallback;

    /// <summary>
    /// Sets a value of type <typeparamref name="T"/> by the section path.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="path">Section path.</param>
    /// <param name="value">Value to set.</param>
    public void SetValue<T>(string path, T value) {
        if (value is null) return;
        if (PropertyBinder.TryGetString(typeof(T), value, out var valueString, out var isQuoted))
            this[path] = isQuoted ? valueString : '=' + valueString;
    }

    /// <summary>
    /// Updates this configuration from <typeparamref name="T"/> object properties.
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    /// <param name="value">Configuration.</param>
    public void Set<T>(T value) where T : class, new() => Binder.Set<T>(this, value);

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
        var prefix = node.Parent is null ? string.Empty : NodePath.GetSectionPath(node.Parent.GetPath()) + ':';
        Path = prefix + node.PropertyName;
        Parent = node.Parent;
    }

    /// <summary>
    /// Gets the node by the key of the configuration section.
    /// </summary>
    /// <param name="key">The key of the configuration section.</param>
    /// <param name="nullNodeIfNotExists">Returns a null node if the property doesn't exist.</param>
    /// <returns>Token matched or null.</returns>
    private JsonNodeSection GetNodeSection(string key, bool nullNodeIfNotExists = false) {
        JsonNode? node = Node, parent = node;
        if (key.Length < 1) return this;
        foreach (var part in NodePath.Split(key)) {
            if (node is null) return nullNodeIfNotExists ? new JsonNodeSection(new NullNode(parent, part)) : Empty;
            else if (node is JsonArray array) {
                parent = node;
                node = int.TryParse(part, out var index) && index >= 0 && index < array.Count ? array[index] : null;
                if (node is null) return nullNodeIfNotExists ? new JsonNodeSection(new NullNode(parent, part)) : Empty;
                if (node is JsonArray or JsonObject) continue;
                else return new JsonNodeSection(node);
            }
            else if (node is JsonObject obj) {
                var exists = obj.TryGetPropertyValue(part, out var value);
                if (value is null) return nullNodeIfNotExists || exists ? new JsonNodeSection(new NullNode(parent, part)) : Empty;
                parent = node;
                node = value;
            }
        }
        return new JsonNodeSection(node!);
    }

    /// <summary>
    /// Gets a JSON value from string.
    /// </summary>
    /// <param name="json">Valid JSON string.</param>
    /// <returns>A <see cref="JsonValue"/> instance.</returns>
    private static JsonValue GetJsonNumberFromString(string json)
        => json.Contains('.')
            ? JsonValue.Create(double.Parse(json, N))
            : json[0] == '-' ? JsonValue.Create(long.Parse(json, N)) : JsonValue.Create(ulong.Parse(json, N));

    /// <summary>
    /// Contains the node used to create this section.
    /// </summary>
    public JsonNode? Node { get; }

    /// <summary>
    /// Contains the parent node of the node used to create this section.
    /// </summary>
    public JsonNode? Parent { get; }

    /// <summary>
    /// Invariant culture format provider for JSON compatible conversions.
    /// </summary>
    private static readonly IFormatProvider N = CultureInfo.InvariantCulture;

    /// <summary>
    /// <see cref="Loader"/> backing field.
    /// </summary>
    private IJsonNodeLoader? _Loader;

    /// <summary>
    /// <see cref="Binder"/> backing field.
    /// </summary>
    private IPropertyBinder? _Binder;

}