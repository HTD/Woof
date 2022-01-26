namespace Woof.Settings.Analysis;

/// <summary>
/// Contains property or value data.
/// </summary>
public struct Property {

    /// <summary>
    /// Provides access to a property metadata.
    /// </summary>
    public PropertyInfo? Info;

    /// <summary>
    /// The type of the property or value.
    /// </summary>
    public Type? Type;

    /// <summary>
    /// Contains the property owner.
    /// </summary>
    public object? Owner;

    /// <summary>
    /// Contains the property value.
    /// </summary>
    public object? Value;

    /// <summary>
    /// Gets a value indicating the property is a collection type.
    /// </summary>
    public bool IsCollection
        => Type is Type type && (typeof(Array).IsAssignableFrom(type) || typeof(IList).IsAssignableFrom(type)) && type != typeof(byte[]);

    /// <summary>
    /// Creates a property data pack.
    /// </summary>
    /// <param name="info">Property metadata.</param>
    /// <param name="owner">Property owner.</param>
    /// <param name="value">Property value.</param>
    public Property(PropertyInfo? info, object? owner, object? value) {
        Info = info;
        Type = info?.PropertyType;
        Owner = owner;
        Value = value;
    }

    /// <summary>
    /// Creates a property data pack.
    /// </summary>
    /// <param name="type">Value type.</param>
    /// <param name="owner">Value owner.</param>
    /// <param name="value">Value.</param>
    public Property(Type? type, object? owner, object? value) {
        Info = null;
        Type = type;
        Owner = owner;
        Value = value;
    }

    /// <summary>
    /// Sets the value of the property.
    /// </summary>
    /// <param name="value">A value to set.</param>
    /// <exception cref="NullReferenceException"><see cref="Info"/> or <see cref="Owner"/> is null.</exception>
    public void SetValue(object? value) {
        if (Info is null || Info.SetMethod is null) return;
        Info.SetValue(Owner, Value = value);
    }

    /// <summary>
    /// Creates the default instance for the property.
    /// </summary>
    public void CreateContainer() => Info?.SetValue(Owner, Value = Activator.CreateInstance(Info!.PropertyType));

    /// <summary>
    /// Creates the collection for the specified path.
    /// </summary>
    /// <param name="path">Path to the collection.</param>
    /// <param name="jsonRoot">JSON root.</param>
    /// <exception cref="InvalidOperationException">Matching <see cref="JsonArray"/> not found.</exception>
    public void CreateCollection(JsonNodePath path, JsonNode jsonRoot) {
        if (typeof(Array).IsAssignableFrom(Type)) {
            var elementType = Type.GetElementType()!; // array always has an element type
            var elementCount = GetJsonArrayCount(jsonRoot, path);
            var array = Array.CreateInstance(elementType, elementCount);
            SetValue(array);
        }
        else if (typeof(IList).IsAssignableFrom(Type)) {
            var elementType = Type.GenericTypeArguments[0]; // list also should have the element type
            var elementCount = GetJsonArrayCount(jsonRoot, path);
            var array = Array.CreateInstance(elementType, elementCount);
            var list = Activator.CreateInstance(Type, new object[] { array });
            SetValue(list);
        }
    }

    /// <summary>
    /// Initializes the collection for the specified path.
    /// Presets arrays and lists.
    /// </summary>
    /// <param name="path">Path to the collection.</param>
    /// <param name="jsonRoot">JSON root.</param>
    /// <exception cref="InvalidOperationException">JSON structure doesn't match the object's structure.</exception>
    public void InitializeCollection(JsonNodePath path, JsonNode jsonRoot) {
        if (Type is null) throw new InvalidOperationException($"Missing property metadata for {path}");
        if (Value is Array array) { // arrays with different sizes are reset if a setter exists
            var elementType = Type.GetElementType()!; // array always has an element type
            var elementCount = GetJsonArrayCount(jsonRoot, path);
            if (array.Length == elementCount) return;
            if (Info!.SetMethod is null)
                throw new InvalidOperationException($"Array property at {path} should have a setter to support a different number of elements");
            SetValue(Array.CreateInstance(elementType, elementCount));
        }
        else if (Value is IList list) { // lists are preset with vaues to behave more like objects
            var elementType = Type.GenericTypeArguments[0]; // list also should have the element type
            var elementCount = GetJsonArrayCount(jsonRoot, path);
            if (list.Count == elementCount) return;
            while (list.Count > elementCount) list.RemoveAt(list.Count - 1);
            var element = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
            while (list.Count < elementCount) list.Add(element);
        }
    }

    /// <summary>
    /// Creates a default property value by creating the appropriate default instance for the type.
    /// </summary>
    /// <param name="path">Property path.</param>
    /// <param name="jsonRoot">JSON root.</param>
    /// <exception cref="InvalidOperationException">Missing metadata for path or matching <see cref="JsonArray"/> not found.</exception>
    public void CreateInstance(JsonNodePath path, JsonNode jsonRoot) {
        if (Info is null) throw new InvalidOperationException($"Missing metadata for {path}");
        if (IsCollection) CreateCollection(path, jsonRoot); else CreateContainer();
    }

    /// <summary>
    /// Initializes the collection at the specified index with the default instance for the collection type.
    /// </summary>
    /// <param name="index">Index.</param>
    public void EnsureInitializedAtIndex(int index) {
        if (Value is Array array && array.GetValue(index) is null) {
            var elementType = Type!.GetElementType()!;
            if (ObjectTree.IsContainer(elementType)) array.SetValue(Activator.CreateInstance(elementType), index);
        }
        if (Value is IList list && list[index] is null) {
            var elementType = Type!.GenericTypeArguments[0];
            if (ObjectTree.IsContainer(elementType)) list[index] = Activator.CreateInstance(elementType);
        }
    }

    /// <summary>
    /// Gets the JSON array element count.
    /// </summary>
    /// <param name="jsonRoot">JSON root.</param>
    /// <param name="path">Path to the array.</param>
    /// <returns>Element count.</returns>
    /// <exception cref="InvalidOperationException">Matchin <see cref="JsonArray"/> not found.</exception>
    private static int GetJsonArrayCount(JsonNode jsonRoot, JsonNodePath path)
        => jsonRoot.Select(path) is JsonArray array
            ? array.Count
            : throw new InvalidOperationException($"Matching JsonArray not found at {path}");

}