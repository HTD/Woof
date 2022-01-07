namespace Woof.Settings;

/// <summary>
/// Contains the data for the <see cref="SpecialAttribute.Resolve"/> event.
/// </summary>
public class SpecialAttributeEventArgs : EventArgs {

    /// <summary>
    /// Gets the special property type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets or sets the value that is resolved by the event handler.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Creates the special attribute event argument for the property type.
    /// </summary>
    /// <param name="type">Property type.</param>
    public SpecialAttributeEventArgs(Type type) => Type = type;

}