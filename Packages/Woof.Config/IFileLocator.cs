namespace Woof.Config;

/// <summary>
/// Defines methods for locating the configuration file path relative to the main executable path.
/// </summary>
public interface IFileLocator {

    /// <summary>
    /// Tries to locate a configuration file for the specified configuration name.
    /// </summary>
    /// <param name="name">Configuration name, or null for default name matching the main executable name.</param>
    /// <returns>A full path to the file, or null if the file is not found.</returns>
    string? Locate(string? name = null);

}