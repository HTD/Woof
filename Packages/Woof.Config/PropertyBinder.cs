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
            var value = FromString(item.Property.PropertyType, configuration[JsonNodeSection.GetKeyPath(item.Path)]);
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
            var path = JsonNodeSection.GetKeyPath(item.Path);
            var section = configuration.GetSection(JsonNodeSection.GetKeyPath(item.Path));
            var propertyValue = item.Property.GetValue(value);
        }
    }

    /// <summary>
    /// Gets the specified type of value from string.
    /// </summary>
    /// <param name="type">Target type.</param>
    /// <param name="value">String value.</param>
    /// <returns>Value object.</returns>
    /// <exception cref="NotSupportedException">Type is unsupported.</exception>
    protected static object? FromString(Type type, string? value)
        => value is null
            ? null
            : type.Name switch {
                nameof(String) => value,
                nameof(Int32) => Int32.Parse(value, CultureInfo.InvariantCulture),
                nameof(Double) => Double.Parse(value, CultureInfo.InvariantCulture),
                nameof(Boolean) => Boolean.Parse(value),
                nameof(DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture),
                nameof(TimeSpan) => TimeSpan.Parse(value, CultureInfo.InvariantCulture),
                nameof(DateOnly) => DateOnly.Parse(value, CultureInfo.InvariantCulture),
                nameof(TimeOnly) => TimeOnly.Parse(value, CultureInfo.InvariantCulture),
                nameof(Uri) => new Uri(value),
                nameof(Guid) => new Guid(value),
                nameof(Int64) => Int64.Parse(value, CultureInfo.InvariantCulture),
                nameof(Int16) => Int16.Parse(value, CultureInfo.InvariantCulture),
                nameof(Byte) => Byte.Parse(value, CultureInfo.InvariantCulture),
                nameof(Single) => Single.Parse(value, CultureInfo.InvariantCulture),
                nameof(Decimal) => Decimal.Parse(value, CultureInfo.InvariantCulture),
                nameof(FileInfo) => new FileInfo(value),
                nameof(DirectoryInfo) => new DirectoryInfo(value),
                "Byte[]" => Convert.FromBase64String(value),
                _ => throw new NotSupportedException($"Binding values of type \"{type.Name}\" is not supported")
            };

}
