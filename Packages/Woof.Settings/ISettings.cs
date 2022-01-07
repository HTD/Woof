namespace Woof.Settings;

/// <summary>
/// Provides methods for loading and saving the settings data.
/// </summary>
/// <typeparam name="TData">Settings data type.</typeparam>
public interface ISettings<TData> {

    /// <summary>
    /// Loads the data from file.
    /// </summary>
    TData Load();

    /// <summary>
    /// Loads the data from file.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the file is loaded.</returns>
    ValueTask<TData> LoadAsync();

    /// <summary>
    /// Saves the data to the default file.
    /// </summary>
    void Save();

    /// <summary>
    /// Saves the data to the default file.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the file is saved.</returns>
    ValueTask SaveAsync();

}
