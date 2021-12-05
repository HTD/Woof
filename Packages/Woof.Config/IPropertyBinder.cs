namespace Woof.Config;

/// <summary>
/// Two way property binder for <see cref="JsonConfig"/>.
/// </summary>
public interface IPropertyBinder { // TODO: implement default binder

    /// <summary>
    /// Gets the configuration record from the <see cref="JsonConfig"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the configuration record.</typeparam>
    /// <param name="configuration">A <see cref="JsonConfig"/> instance.</param>
    /// <returns>Configuration record.</returns>
    public T Get<T>(JsonConfig configuration) where T : class;

    /// <summary>
    /// Updates the <see cref="JsonConfig"/> instance with the given configuration record value.<br/>
    /// It will work only if the corresponding <see cref="JsonConfig"/> properties exist and are not nullable.<br/>
    /// Consider making properties not nullable to be able to save them.
    /// </summary>
    /// <typeparam name="T">The type of the configuration record.</typeparam>
    /// <param name="configuration">A <see cref="JsonConfig"/> instance.</param>
    /// <param name="value">Configuration record.</param>
    /// <returns></returns>
    public void Set<T>(JsonConfig configuration, T value) where T : class;

}