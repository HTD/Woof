namespace Woof.Settings;

/// <summary>
/// Provides a two way property binder for the <see cref="JsonNode"/>.
/// </summary>
public static class JsonNodeBinder {

    /// <summary>
    /// Sets the object instance properties with the properties of the <see cref="JsonNode"/> instance.
    /// </summary>
    /// <param name="localRoot">The <see cref="JsonNode"/> instance to bind.</param>
    /// <param name="target">The object to bind.</param>
    public static void Bind(this JsonNode localRoot, object target) {
        if (target is null) return;
        var targetTree = new ObjectTree(target);
        var jsonLeaves = JsonNodeTree.Traverse(localRoot).ToArray();
        foreach (var node in jsonLeaves) targetTree.SetProperty(node);
        var targetLeaves = targetTree.Traverse().ToArray();
        var toBeTrimmed = targetLeaves.Where(t => !jsonLeaves.Any(j => j.Path.Equals(t.Path)));
        foreach (var node in toBeTrimmed) targetTree.Clear(node);
        foreach (var node in targetLeaves) targetTree.SetSpecialProperty(node);
    }

    /// <summary>
    /// Attempts to bind the <see cref="JsonNode"/> instance to a new instance of type T.<br/>
    /// If this <see cref="JsonNode"/> is a <see cref="JsonValue"/>, that will be used.<br/>
    /// Otherwise binding by matching property names against <see cref="JsonNode"/> keys recursively.<br/>
    /// </summary>
    /// <param name="localRoot">A <see cref="JsonNode"/> instance to bind.</param>
    /// <param name="type">The type of the new instance to bind.</param>
    /// <returns>The new instance if successful, null otherwise.</returns>
    public static object? Get(this JsonNode localRoot, Type type) {
        if (localRoot is JsonValue valueNode && valueNode.TryConvert(type, out var value)) return value;
        var target = Activator.CreateInstance(type);
        if (target is null) return null;
        localRoot.Bind(target);
        return target;
    }

    /// <summary>
    /// Attempts to bind the <see cref="JsonNode"/> instance to a new instance of type <typeparamref name="T"/>.<br/>
    /// If this <see cref="JsonNode"/> is a <see cref="JsonValue"/>, that will be used.<br/>
    /// Otherwise binding by matching property names against <see cref="JsonNode"/> keys recursively.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <param name="localRoot">A <see cref="JsonNode"/> instance to bind.</param>
    /// <returns>The new instance of <typeparamref name="T"/>.</returns>
    public static T Get<T>(this JsonNode localRoot) where T : new() {
        if (localRoot is JsonValue valueNode && valueNode.TryConvert<T>(out var value)) return value!;
        var target = new T();
        localRoot.Bind(target);
        return target;
    }

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the value to convert.</param>
    /// <returns>The converted value or null.</returns>
    public static object? GetValue(this JsonNode node, Type type, JsonNodePath path)
        => (node.GetValueNode(path) is JsonValue valueNode && valueNode.TryConvert(type, out var value)) ? value : default;

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    public static object GetValue(this JsonNode node, Type type, JsonNodePath path, object defaultValue)
        => (node.GetValueNode(path) is JsonValue valueNode && valueNode.TryConvert(type, out var value) && value is not null) ? value : defaultValue;

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="node">The <see cref="JsonNode"/> instance.</param>
    /// <param name="path">The path of the value to convert.</param>
    /// <returns>The converted value.</returns>
    public static T GetValue<T>(this JsonNode node, JsonNodePath path)
        => (node.GetValueNode(path) is JsonValue valueNode && valueNode.TryConvert<T>(out var value)) ? value! : default!;

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="node">The <see cref="JsonNode"/> instance.</param>
    /// <param name="path">The path of the value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    public static T GetValue<T>(this JsonNode node, JsonNodePath path, T defaultValue)
        => (node.GetValueNode(path) is JsonValue valueNode && valueNode.TryConvert<T>(out var value)) ? value ?? defaultValue : defaultValue;

    /// <summary>
    /// Updates the <see cref="JsonNode"/> instance with the given object properties.<br/>
    /// </summary>
    /// <param name="localRoot">A <see cref="JsonNode"/> instance.</param>
    /// <param name="value">The object to bind.</param>
    public static void Set(this JsonNode localRoot, object value) {
        var targetTree = new JsonNodeTree(localRoot);
        foreach (var node in ObjectTree.Traverse(value)) targetTree.SetProperty(node, value);
    }

    /// <summary>
    /// Updates the <see cref="JsonNode"/> instance with the given object properties.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the object to bind.</typeparam>
    /// <param name="localRoot">A <see cref="JsonNode"/> instance.</param>
    /// <param name="value">The object to bind.</param>
    public static void Set<T>(this JsonNode localRoot, T value) where T : class, new() {
        var targetTree = new JsonNodeTree(localRoot);
        foreach (var node in ObjectTree.Traverse(value)) targetTree.SetProperty(node, value);
    }

    /// <summary>
    /// Updates the node value at the specified path.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> instance.</param>
    /// <param name="path">The path of the value to update.</param>
    /// <param name="value">The new value to set.</param>
    public static void SetValue(this JsonNode node, JsonNodePath path, object value) {
        if (JsonValueBuilder.TryConvert(value.GetType(), value, out var valueNode))
            node.SetValueNode(path, valueNode);
    }

    /// <summary>
    /// Updates the node value at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the new value.</typeparam>
    /// <param name="node">The <see cref="JsonNode"/> instance.</param>
    /// <param name="path">The path of the value to update.</param>
    /// <param name="value">The new value to set.</param>
    public static void SetValue<T>(this JsonNode node, JsonNodePath path, T value) {
        if (JsonValueBuilder.TryConvert(typeof(T), value, out var valueNode))
            node.SetValueNode(path, valueNode);
    }

}