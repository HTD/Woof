namespace Woof.Config;

/// <summary>
/// Provides the <see cref="IPropertyBinder"/> extensions to the <see cref="JsonNodeSection"/> type.
/// </summary>
public static class JsonNodeSectionBinder {

    /// <summary>
    /// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="target">The object to bind.</param>
    public static void Bind(this JsonNodeSection section, object target)
        => section.Binder.Bind(section, target);

    /// <summary>
    /// Attempts to bind the configuration instance to a new instance of type T.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="type">The type of the new instance to bind.</param>
    /// <returns>The new instance if successful, null otherwise.</returns>
    public static object? Get(this JsonNodeSection section, Type type)
        => section.Binder.Get(section, type);

    /// <summary>
    /// Attempts to bind the configuration instance to a new instance of type <typeparamref name="T"/>.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <returns>The new instance of <typeparamref name="T"/> if successful, default(<typeparamref name="T"/>) otherwise.</returns>
    public static T Get<T>(this JsonNodeSection section) where T : class, new()
        => section.Binder.Get<T>(section);

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <returns>The converted value.</returns>
    public static object? GetValue(this JsonNodeSection section, Type type, string? path = null)
        => section.Binder.GetValue(section, type, path);

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    public static object GetValue(this JsonNodeSection section, Type type, string? path, object defaultValue)
        => section.Binder.GetValue(section, type, path, defaultValue);

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <returns>The converted value.</returns>
    public static T GetValue<T>(this JsonNodeSection section, string? path = null)
        => section.Binder.GetValue<T>(section, path);

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    public static T GetValue<T>(this JsonNodeSection section, string path, T defaultValue)
        => section.Binder.GetValue<T>(section, path, defaultValue);

    /// <summary>
    /// Updates the <see cref="JsonNodeSection"/> instance with the <typeparamref name="T"/> instance property values.
    /// </summary>
    /// <typeparam name="T">The type of the instance to bind.</typeparam>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="value">A <typeparamref name="T"/> instance to bind.</param>
    public static void Set<T>(this JsonNodeSection section, T value) where T : class, new() => section.Binder.Set(section, value);

    /// <summary>
    /// Updates the section value with the specified path.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to update.</param>
    /// <param name="value">The new value to set.</param>
    public static void SetValue(this JsonNodeSection section, string path, object value)
        => section.Binder.SetValue(section, path, value);

    /// <summary>
    /// Updates the section value with the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the new value.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to update.</param>
    /// <param name="value">The new value to set.</param>
    public static void SetValue<T>(this JsonNodeSection section, string path, T value)
        => section.Binder.SetValue<T>(section, path, value);

}
