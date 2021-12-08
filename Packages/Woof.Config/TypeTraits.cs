using System.Collections;

namespace Woof.Config.Internals;

/// <summary>
/// Defines
/// </summary>
public static class TypeTraits {

    /// <summary>
    /// Checks if the type is a collection type.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <returns>True if the type implements <see cref="ICollection"/> interface.</returns>
    public static bool IsCollection(this Type type) => type.GetInterface(nameof(ICollection)) != null;

    /// <summary>
    /// Gets the element type of the collection type if any.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <returns>Element type.</returns>
    public static Type? GetItemType(this Type type) => type.GetElementType() ?? type.GenericTypeArguments.FirstOrDefault();

}
