namespace Woof.Config.Internals;

// TODO: Use PropertyTraverser to implement internal getter instead of the default extension.
// TODO: Use PropertyTraverser to update the JsonNode before saving.
// TODO: Use PrppertyTraverser and IPropertyBinder make an extension method for JsonConfig getting values from AKV.
// TODO: Remove Woof.Config.AKV.Protected, add data protection directly to Woof.Config.AKV as extension method.

/// <summary>
/// BFS object graph traverser.
/// </summary>
internal static class PropertyTraverser {

    /// <summary>
    /// Traverses through the object's graph in BFS order returning all public properties
    /// that does not point to the object within the root's namespace.
    /// </summary>
    /// <param name="root">Root object to traverse.</param>
    /// <returns>Enumeration of <see cref="PropertyGraphItem"/>.</returns>
    public static IEnumerable<PropertyGraphItem> Traverse(object root) {
        var type = root.GetType();
        var rootNamespace = type.Namespace ?? string.Empty;
        var q = new Queue<PropertyGraphItem>();
        foreach (var property in type.GetProperties())
            q.Enqueue(new() { Owner = root, Property = property, Path = property.Name });
        PropertyGraphItem item;
        object? value;
        while (q.Count > 0) {
            item = q.Dequeue();
            type = item.Property.PropertyType;
            if ((type.Namespace ?? string.Empty).StartsWith(rootNamespace, StringComparison.Ordinal)) {
                value = item.Property.GetValue(item.Owner);
                if (value is null) continue;
                foreach (var property in value.GetType().GetProperties())
                    q.Enqueue(new() { Owner = value, Property = property, Path = item.Path + ':' + property.Name });
            }
            else yield return item;
        }
    }

}

/// <summary>
/// An object's property tree item that allows setting the property value by it's path in <see cref="JsonNode"/> path format.
/// </summary>
internal struct PropertyGraphItem {

    /// <summary>
    /// The object that is the owner of the property.
    /// </summary>
    public object Owner;

    /// <summary>
    /// The object's property.
    /// </summary>
    public PropertyInfo Property;

    /// <summary>
    /// The property path in the object's graph in <see cref="IConfiguration"/> format.
    /// </summary>
    public string Path;

}