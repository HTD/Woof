namespace Woof.Config;

/// <summary>
/// An object's property tree item that allows setting the property value by it's path in <see cref="JsonNode"/> path format.
/// </summary>
public struct PropertyGraphItem {

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

    /// <summary>
    /// Creates a new <see cref="PropertyGraphItem"/>.
    /// </summary>
    /// <param name="owner">Property owner.</param>
    /// <param name="property">Property.</param>
    /// <param name="path">Configuration path.</param>
    public PropertyGraphItem(object owner, PropertyInfo property, string path) {
        Owner = owner;
        Property = property;
        Path = path;
    }

}