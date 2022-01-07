namespace Woof.Settings;

/// <summary>
/// The property decorated with this attribute is special and it's not bound directly.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class SpecialAttribute : Attribute {

    /// <summary>
    /// Occurs when the value of a special property needs to be resolved.
    /// </summary>
    public static event EventHandler<SpecialAttributeEventArgs>? Resolve;

    /// <summary>
    /// Tries to resolve a value for the special property.
    /// </summary>
    /// <param name="type">Property type.</param>
    /// <returns>Resolved value or null.</returns>
    internal object? OnResolve(Type type) {
        if (Resolve is null) return null;
        var args = new SpecialAttributeEventArgs(type);
        Resolve(this, args);
        return args.Value;
    }

}