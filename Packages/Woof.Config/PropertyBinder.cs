namespace Woof.Config;

/// <summary>
/// Binds the compatible objects properties to <see cref="JsonNodeSection"/> instance.
/// </summary>
public class PropertyBinder : IPropertyBinder {

    /// <summary>
    /// Gets the configuration record from the <see cref="JsonNodeSection"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the configuration record.</typeparam>
    /// <param name="configuration">A <see cref="JsonConfig"/> instance.</param>
    /// <returns>Configuration record.</returns>
    public T Get<T>(JsonNodeSection configuration) where T : class, new() {
        var target = new T();
        foreach (var item in Traverse(target)) {
            if (configuration[item.Path] is string stringValue && TryGetValue(item.Property.PropertyType, stringValue, out var value))
                item.Property.SetValue(item.Owner, value);
        }
        return target;
    }

    /// <summary>
    /// Updates the <see cref="JsonNodeSection"/> instance with the given configuration record value.<br/>
    /// It will work only if the corresponding <see cref="JsonNodeSection"/> properties exist and are not nullable.<br/>
    /// Consider making properties not nullable to be able to save them.
    /// </summary>
    /// <typeparam name="T">The type of the configuration record.</typeparam>
    /// <param name="configuration">A <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="value">Configuration record.</param>
    public void Set<T>(JsonNodeSection configuration, T value) where T : class, new() {
        foreach (var item in Traverse(value)) {
            var propertyValue = item.Property.GetValue(value);
            if (propertyValue is null) continue;
            if (TryGetString(item.Property.PropertyType, propertyValue, out var stringValue, out var isQuoted))
                configuration[item.Path] = isQuoted ? stringValue : '=' + stringValue;
        }
    }

    /// <summary>
    /// Tries to get the value of the specified type from string.
    /// </summary>
    /// <param name="type">Type to convert.</param>
    /// <param name="stringValue">String value.</param>
    /// <param name="value">Converted value.</param>
    /// <returns>True if value was converted using available conversions.</returns>
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
    /// Tries to get the value of the specified type as string.
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
            if (type.BaseType != typeof(Enum) && (type.Namespace ?? string.Empty).StartsWith(rootNamespace, StringComparison.Ordinal)) {
                value = item.Property.GetValue(item.Owner);
                if (value is null) continue;
                foreach (var property in value.GetType().GetProperties(BindingFlags))
                    q.Enqueue(new(value, property, item.Path + ':' + property.Name));
            }
            else yield return item;
        }
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