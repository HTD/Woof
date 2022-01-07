namespace Woof.Settings;

/// <summary>
/// Defines a method for locating the configuration file.
/// </summary>
public interface ILocator {

    /// <summary>
    /// Locates a settings file for the specified settings name.
    /// </summary>
    /// <param name="name">Settings file base name (no extension).</param>
    /// <returns>A tuple with the full path to the file and a flag indicating whether the file exists.</returns>
    (string path, bool exists) Locate(string name);

    /// <summary>
    /// Finds the primary and secondary location for the new settings file. One of these should be used to save a new file.
    /// </summary>
    /// <param name="name">Settings file base name (no extension).</param>
    /// <returns>
    /// A tuple containing the primary and secondary paths.
    /// The caller can decide whether to use the secondary path if the primary path is not writeable.
    /// </returns>
    (string primaryPath, string secondaryPath) LocateNew(string name);

}