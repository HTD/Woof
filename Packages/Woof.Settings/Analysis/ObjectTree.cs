namespace Woof.Settings.Analysis;

/// <summary>
/// A class enumerating the <see cref="object"/> property tree.
/// </summary>
public class ObjectTree {

    /// <summary>
    /// Gets the target object.
    /// </summary>
    public object Target { get; }

    /// <summary>
    /// Creates a property tree metadata over an object.
    /// </summary>
    /// <param name="target">Target object.</param>
    public ObjectTree(object target) => Target = target;

    /// <summary>
    /// Enumerates all <see cref="object"/> properties, whose values can be represented as non-complex, <see cref="JsonValue"/> values.
    /// </summary>
    /// <param name="root">Root object.</param>
    /// <returns>Enumeration of all directly convertible properties in BFS order.</returns>
    public static IEnumerable<ObjectTreeNode> Traverse(object root) {
        var type = root.GetType();
        Queue<ObjectTreeNode> q = new();
        ObjectTreeNode node;
        object? content;
        foreach (var property in type.GetProperties(BindingFlags)) q.Enqueue(new ObjectTreeNode(root, property));
        while (q.Count > 0) {
            node = q.Dequeue();
            content = node.Content!;
            if (content is Array array and not byte[])
                for (int i = 0, n = array.Length; i < n; i++) q.Enqueue(new ObjectTreeNode(node, i));
            else if (content is IList list and not byte[])
                for (int i = 0, n = list.Count; i < n; i++) q.Enqueue(new ObjectTreeNode(node, i));
            else if (IsContainer(node.Type))
                foreach (var property in node.Type!.GetProperties(BindingFlags))
                    q.Enqueue(new ObjectTreeNode(node, property));
            else yield return node;
        }
    }

    /// <summary>
    /// Enumerates all <see cref="Target"/> properties, whose values can be represented as non-complex, <see cref="JsonValue"/> values.
    /// </summary>
    /// <returns>Enumeration of all directly convertible properties in BFS order.</returns>
    public IEnumerable<ObjectTreeNode> Traverse() => Traverse(Target);

    /// <summary>
    /// Gets the property of the object by the path to the property.
    /// </summary>
    /// <param name="root">Root node.</param>
    /// <param name="path">Path to the property.</param>
    /// <returns>A <see cref="Property"/> instance or null if the property doesn't exist.</returns>
    public static Property GetPropertyByPath(object root, JsonNodePath path) {
        var currentTarget = root;
        if (root is null) return new Property();
        PropertyInfo? currentProperty = null;
        Type? elementType = null;
        object? currentOwner = null;
        foreach (var part in path.Parts) {
            if (part.Key == "$") continue; // root is not a property, it's the owner.
            currentOwner = currentTarget;
            var index = part.Index;
            elementType = null;
            if (index >= 0) {
                currentProperty = null;
                if (currentTarget is Array array) {
                    elementType = array.GetType().GetElementType()!;
                    currentTarget = array.GetValue(index);
                }
                else if (currentTarget is IList list) {
                    elementType = list.GetType().GenericTypeArguments[0];
                    currentTarget = index < list.Count ? list[index] : null;
                }
            }
            else {
                currentProperty =
                    currentTarget?
                    .GetType()
                    .GetProperties(BindingFlags)
                    .FirstOrDefault(i => i.Name.Equals(part.Key, StringComparison.OrdinalIgnoreCase));
                currentTarget = currentProperty?.GetValue(currentTarget);
            }
            if (currentTarget is null) return new Property(currentProperty, currentOwner, currentTarget);
        }
        return
            currentProperty is not null
                ? new Property(currentProperty, currentOwner, currentTarget)
                : new Property(elementType, currentOwner, currentTarget);

    }

    /// <summary>
    /// Gets the property of the object by the path to the property.
    /// </summary>
    /// <param name="path">Path to the property.</param>
    /// <returns>A <see cref="Property"/> instance or null if the property doesn't exist.</returns>
    public Property GetPropertyByPath(JsonNodePath path) => GetPropertyByPath(Target, path);

    /// <summary>
    /// Clears the specified node from it's parent if possible.
    /// </summary>
    /// <param name="node">The node to clear.</param>
    public void Clear(ObjectTreeNode node) {
        var info = node.Info;
        if (info?.SetMethod is null) return;
        Property? parent = node.Path.Parent is null ? null : GetPropertyByPath(node.Path.Parent);
        if (parent is null) return;
        object? defaultValue = info.PropertyType.IsValueType ? Activator.CreateInstance(info.PropertyType) : null;
        info.SetValue(parent.Value.Value, defaultValue);
    }

    /// <summary>
    /// If any property referenced by a path key is null, it is created when there is enough metadata to do so.
    /// </summary>
    /// <param name="path">Parent (container) node path.</param>
    /// <param name="jsonRoot">JSON root.</param>
    /// <exception cref="InvalidOperationException">A mismatch between object tree and the JsonNode.</exception>
    public void EnsureParentExists(JsonNodePath path, JsonNode jsonRoot) {
        if (path is null || path.Length < 2) return; // root and direct properties always exist
        var parts = path.Parts.Skip(1); // so we have parts that might need an initialization
        foreach (var part in parts) {
            var index = part.Index;
            if (index >= 0) GetPropertyByPath(part.Parent!).EnsureInitializedAtIndex(part.Index);
            else {
                var property = GetPropertyByPath(part);
                if (property.Value is null) property.CreateInstance(part, jsonRoot);
                else if (property.Value is Array or IList) property.InitializeCollection(part, jsonRoot);
            }
        }
    }

    /// <summary>
    /// Sets the property from <see cref="JsonNodeTreeNode"/>.
    /// </summary>
    /// <param name="node">Property node.</param>
    public void SetProperty(JsonNodeTreeNode node) {
        EnsureParentExists(node.Path.Parent!, node.Root!);
        var property = GetPropertyByPath(node.Path);
        var index = node.Path.Index;
        var value =
            property.Type is Type type &&
            node.Value is JsonValue jsonValue &&
            jsonValue.TryConvert(type, out var converted) ? converted : null;
        if (index < 0) {
            property.SetValue(value);
            return;
        }
        var collectionPath = node.Path.Parent!;
        var collection = GetPropertyByPath(collectionPath).Value;
        if (collection is Array array) array.SetValue(value, index);
        if (collection is IList list) {
            if (index < list.Count) list[index] = value;
            else list.Add(value);
        }
    }

    /// <summary>
    /// Sets a special property resolved outside the JSON.
    /// </summary>
    /// <param name="node">Object tree node.</param>
    public void SetSpecialProperty(ObjectTreeNode node) {
        if (node.Info?.GetCustomAttribute<SpecialAttribute>() is not SpecialAttribute special) return;
        var value = special.OnResolve(node.Info.PropertyType);
        if (value is null) return;
        var property = GetPropertyByPath(node.Path);
        property.SetValue(value);
    }

    /// <summary>
    /// Determines if the specific type is a container type.
    /// </summary>
    /// <param name="type">Tested type.</param>
    /// <returns>True if the type is not a value, but an object container / node.</returns>
    public static bool IsContainer(Type? type) =>
        type is not null && // is not null
        type.BaseType != typeof(Enum) && // is not an enumeration
        !typeof(ICollection).IsAssignableFrom(type) && // is not a collection
        !ValueConversions.Default.ContainsKey(type); // is not convertible to a single value

    /// <summary>
    /// Gets or sets the property binding flags. The public instance members are searched.
    /// </summary>
    public static BindingFlags BindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;

}