﻿namespace Woof.Config;

/// <summary>
/// Defines a two way property binder for <see cref="JsonNodeSection"/>.
/// </summary>
public interface IPropertyBinder {

    /// <summary>
    /// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="target">The object to bind.</param>
    void Bind(JsonNodeSection section, object target);

    /// <summary>
    /// Attempts to bind the configuration instance to a new instance of type T.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="type">The type of the new instance to bind.</param>
    /// <returns>The new instance if successful, null otherwise.</returns>
    object? Get(JsonNodeSection section, Type type);

    /// <summary>
    /// Attempts to bind the configuration instance to a new instance of type <typeparamref name="T"/>.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <returns>The new instance of <typeparamref name="T"/> if successful, default(<typeparamref name="T"/>) otherwise.</returns>
    T Get<T>(JsonNodeSection section) where T : class, new();

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <returns>The converted value.</returns>
    object? GetValue(JsonNodeSection section, Type type, string? path = null);

    /// <summary>
    /// Extracts the value with the specified path and converts it to the specified type.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="type">The type to convert the value to.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    object GetValue(JsonNodeSection section, Type type, string? path, object defaultValue);

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <returns>The converted value.</returns>
    T GetValue<T>(JsonNodeSection section, string? path = null);

    /// <summary>
    /// Extracts the value with the specified path and converts it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to convert.</param>
    /// <param name="defaultValue">The default value to use if no value is found.</param>
    /// <returns>The converted value.</returns>
    T GetValue<T>(JsonNodeSection section, string? path, T defaultValue);

    /// <summary>
    /// Updates the <see cref="JsonNodeSection"/> instance with the <typeparamref name="T"/> instance property values.
    /// </summary>
    /// <typeparam name="T">The type of the instance to bind.</typeparam>
    /// <param name="section">A <see cref="JsonNodeSection"/> instance to bind.</param>
    /// <param name="value">A <typeparamref name="T"/> instance to bind.</param>
    public void Set<T>(JsonNodeSection section, T value) where T : class, new();

    /// <summary>
    /// Updates the section value with the specified path.
    /// </summary>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to update.</param>
    /// <param name="value">The new value to set.</param>
    void SetValue(JsonNodeSection section, string? path, object value);

    /// <summary>
    /// Updates the section value with the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the new value.</typeparam>
    /// <param name="section">The <see cref="JsonNodeSection"/> instance.</param>
    /// <param name="path">The path of the configuration section's value to update.</param>
    /// <param name="value">The new value to set.</param>
    void SetValue<T>(JsonNodeSection section, string? path, T value);

}