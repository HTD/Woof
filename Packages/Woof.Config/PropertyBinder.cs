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
        var properties = PropertyTraverser.Traverse(target);
        foreach (var item in properties) {
            var valueString = configuration[JsonNodeSection.GetKeyPath(item.Path)];
            var value = valueString is null ? null : Conversions[item.Property.PropertyType].Parse(valueString);
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
        var properties = PropertyTraverser.Traverse(value);
        foreach (var item in properties) {
            var fullKeyPath = JsonNodeSection.GetKeyPath(item.Path);
            var lastSeparatorOffset = fullKeyPath.LastIndexOf(':');
            var keyPath = fullKeyPath[(lastSeparatorOffset + 1)..];
            var sectionPath = lastSeparatorOffset < 0 ? null : fullKeyPath[0..lastSeparatorOffset];
            var propertyValue = item.Property.GetValue(value);
            string? valueString = null;
            if (propertyValue is not null) {
                var conversion = Conversions[item.Property.PropertyType];
                valueString = conversion.IsQuoted ? string.Empty : "=";
                valueString += conversion.GetString(propertyValue);
            }
            if (sectionPath is null) configuration[keyPath] = valueString;
            else configuration.GetSection(sectionPath)[keyPath] = valueString;
        }
    }

    /// <summary>
    /// Provides supported value conversions.
    /// </summary>
    public static ValueConversions Conversions { get; } = ValueConversions.Default;

}