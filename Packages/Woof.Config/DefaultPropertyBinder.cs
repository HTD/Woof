namespace Woof.Config;

/// <summary>
/// Provides a default two way property binder for <see cref="JsonNodeSection"/>.
/// </summary>
public class DefaultPropertyBinder : IPropertyBinder {

    /// <summary>
    /// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="localRoot">The <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="target">The object to bind.</param>
    public void Bind(JsonNodeSection localRoot, object target) {
        foreach (var item in Traverse(target)) {
            var section = localRoot.Select(item.Path);
            if (section.NodeType == JsonNodeType.Array) GetCollection(section, item);
            else if (section.Value is string stringValue && TryGetValue(item.Property.PropertyType, stringValue, out var value))
                item.Property.SetValue(item.Owner, value);
        }
    }

    /// <summary>
    /// Attempts to bind the configuration instance to a new instance of type T.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="localRoot">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="type">The type of the new instance to bind.</param>
    /// <returns>The new instance if successful, null otherwise.</returns>
    public object? Get(JsonNodeSection localRoot, Type type) {
        if (localRoot.NodeType == JsonNodeType.Value) return localRoot.GetValue(type);
        var target = Activator.CreateInstance(type);
        if (target is null) throw new InvalidOperationException($"Can't create an instance of {type.Name}");
        foreach (var item in Traverse(target)) {
            var section = localRoot.Select(item.Path);
            if (section.NodeType == JsonNodeType.Array) GetCollection(section, item);
            else if (section.Value is string stringValue && TryGetValue(item.Property.PropertyType, stringValue, out var value))
                item.Property.SetValue(item.Owner, value);
        }
        return target;
    }

    /// <summary>
    /// Attempts to bind the configuration instance to a new instance of type <typeparamref name="T"/>.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <typeparam name="T">The type of the configuration record.</typeparam>
    /// <param name="localRoot">A <see cref="JsonNodeSection"/> instance.</param>
    /// <returns>Configuration object.</returns>
    public T Get<T>(JsonNodeSection localRoot) where T : class, new() {
        if (localRoot.NodeType == JsonNodeType.Value) return localRoot.GetValue<T>();
        var target = new T();
        foreach (var item in Traverse(target)) {
            var section = localRoot.Select(item.Path);
            if (section.NodeType == JsonNodeType.Array) GetCollection(section, item);
            else if (section.Value is string stringValue && TryGetValue(item.Property.PropertyType, stringValue, out var value))
                item.Property.SetValue(item.Owner, value);
        }
        return target;
    }

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <returns>The converted value.</returns>
    public object? GetValue(JsonNodeSection section, Type type, string? path = null)
        => (path is null ? section.Value : section.GetSection(path).Value) is string stringValue &&
            TryGetValue(type, stringValue, out var value) && value is not null ? value : default;

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    public object GetValue(JsonNodeSection section, Type type, string? path, object defaultValue)
        => (path is null ? section.Value : section.GetSection(path).Value) is string stringValue &&
            TryGetValue(type, stringValue, out var value) && value is not null ? value : defaultValue;

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <returns>The converted value.</returns>
    public T GetValue<T>(JsonNodeSection section, string? path = null)
        => (path is null ? section.Value : section.GetSection(path).Value) is string stringValue &&
            TryGetValue(typeof(T), stringValue, out var value) && value is not null ? (T)value! : default!;

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    public T GetValue<T>(JsonNodeSection section, string? path, T defaultValue)
        => (path is null ? section.Value : section.GetSection(path).Value) is string stringValue &&
            TryGetValue(typeof(T), stringValue, out var value) && value is not null ? (T)value! : defaultValue!;

    /// <summary>
    /// Updates the <see cref="JsonNodeSection"/> instance with the given configuration object value.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the configuration record.</typeparam>
    /// <param name="localRoot">A <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="value">Configuration object.</param>
    public void Set<T>(JsonNodeSection localRoot, T value) where T : class, new() {
        foreach (var item in Traverse(value)) {
            if (item.Property.PropertyType != typeof(byte[]) && item.Property.PropertyType.GetInterface(nameof(ICollection)) is not null)
                SetCollection(localRoot.Select(item.Path), item);
            else {
                var propertyValue = item.Property.GetValue(item.Owner);
                if (propertyValue is null) continue;
                if (TryGetString(item.Property.PropertyType, propertyValue, out var stringValue, out var isQuoted))
                    localRoot[item.Path] = isQuoted ? stringValue : '=' + stringValue;
            }
        }
    }

    /// <summary>
    /// Updates the section value with the specified path.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to update.</param>
    /// <param name="value">The new value to set.</param>
    public void SetValue(JsonNodeSection section, string? path, object value) {
        if (value is null) return;
        if (TryGetString(value.GetType(), value, out var valueString, out var isQuoted))
            section[path ?? ""] = isQuoted ? valueString : '=' + valueString;
    }

    /// <summary>
    /// Updates the section value with the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the new value.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to update.</param>
    /// <param name="value">The new value to set.</param>
    public void SetValue<T>(JsonNodeSection section, string? path, T value) {
        if (value is null) return;
        if (TryGetString(typeof(T), value, out var valueString, out var isQuoted))
            section[path ?? ""] = isQuoted ? valueString : '=' + valueString;
    }

    /// <summary>
    /// Tries to get the <paramref name="value"/> of the specified <paramref name="type"/> from string.
    /// </summary>
    /// <param name="type">Type to convert.</param>
    /// <param name="stringValue">String value.</param>
    /// <param name="value">Converted value.</param>
    /// <returns>True if the <paramref name="value"/> was converted using available conversions.</returns>
    public static bool TryGetValue(Type type, string stringValue, out object? value) {
        if (type.BaseType == typeof(Enum)) {
            value = Enum.Parse(type, stringValue);
            return true;
        }
        try {
            value = Conversions[type].Parse(stringValue);
            return true;
        }
        catch {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the <paramref name="stringValue"/> of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to convert.</param>
    /// <param name="value">Value to convert.</param>
    /// <param name="stringValue">String value.</param>
    /// <param name="isQuoted">True if the string should be quoted in JSON.</param>
    /// <returns>True if value was converted using available conversions.</returns>
    public static bool TryGetString(Type type, object value, out string? stringValue, out bool isQuoted) {
        if (type.BaseType == typeof(Enum)) {
            stringValue = value.ToString();
            return isQuoted = true;
        }
        try {
            var (Parse, GetString, IsQuoted) = Conversions[type];
            stringValue = GetString(value);
            isQuoted = IsQuoted;
            return true;
        }
        catch {
            stringValue = null;
            isQuoted = false;
            return false;
        }
    }

    /// <summary>
    /// Traverses through the object's graph in BFS order returning all public properties
    /// that does not point to the object within the root's namespace.
    /// </summary>
    /// <param name="root">Root object to traverse.</param>
    /// <returns>All object graph tree leaves in BFS order.</returns>
    public static IEnumerable<PropertyGraphItem> Traverse(object root) {
        var type = root.GetType();
        var rootNamespace = type.Namespace ?? string.Empty;
        var q = new Queue<PropertyGraphItem>();
        foreach (var property in type.GetProperties(BindingFlags))
            q.Enqueue(new(root, property, property.Name));
        PropertyGraphItem item;
        object? value;
        while (q.Count > 0) {
            item = q.Dequeue();
            type = item.Property.PropertyType;
            if (IsContainer(type)) {
                value = item.Property.GetValue(item.Owner);
                if (value is null) continue; // ignore empty containers.
                foreach (var property in value.GetType().GetProperties(BindingFlags))
                    q.Enqueue(new(value, property, item.Path + ':' + property.Name));
            }
            else yield return item;
        }
    }

    /// <summary>
    /// Determines if the specific type is a container type.
    /// </summary>
    /// <param name="type">Tested type.</param>
    /// <returns>True if the type is not a value, but an object container / node.</returns>
    private static bool IsContainer(Type type) =>
        type.BaseType != typeof(Enum) &&
        type.GetInterface(nameof(ICollection)) is null &&
        !Conversions.ContainsKey(type);

    /// <summary>
    /// Gets the items collection from the source section to the target object property.
    /// // TODO: Lists simple, Array of sections, List of sections.
    /// </summary>
    /// <param name="section">An array section.</param>
    /// <param name="item">Target item.</param>
    private static void GetCollection(JsonNodeSection section, PropertyGraphItem item) {
        var type = item.Property.PropertyType;
        var collection = item.Property.GetValue(item.Owner) as ICollection;
        var elementType = type.GenericTypeArguments.FirstOrDefault() ?? type.GetElementType();
        if (elementType is null) throw new InvalidOperationException("Cannot determine the collection element type");
        var isValueElementType = Conversions.ContainsKey(elementType);
        var arrayNode = section.ArrayNode;
        if (collection is Array array) {
            if (arrayNode.Count != array.Length) throw new InvalidOperationException("Array length mismatch between JSON and binding");
            for (int i = 0, n = array.Length; i < n; i++) {
                var elementNode = arrayNode[i];
                var elementSection = elementNode is null ? null : new JsonNodeSection(elementNode);
                if (IsContainer(elementType) && elementSection is not null) {
                    var value = elementSection.Get(elementType);
                }
                else if (isValueElementType) {
                    if ((elementNode as JsonValue)?.ToString() is string stringValue && TryGetValue(elementType, stringValue, out var value))
                        array.SetValue(value, i);
                }
            }
        }

    }

    /// <summary>
    /// Sets the array sections with the items from the source object property.
    /// </summary>
    /// <param name="section">Target array section.</param>
    /// <param name="item">Source item.</param>
    private static void SetCollection(JsonNodeSection section, PropertyGraphItem item) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets or sets the property binding flags.
    /// </summary>
    public static BindingFlags BindingFlags { get; } = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// Provides supported value conversions.
    /// </summary>
    public static ValueConversions Conversions { get; } = ValueConversions.Default;

}