namespace Woof.Settings;

/// <summary>
/// Contains protected settings file extension.
/// </summary>
public static class SettingsProtected {

    /// <summary>
    /// Protected file extension.
    /// </summary>
    public const string Extension = ".data";

    /// <summary>
    /// Tests the path for the encrypted file extension.
    /// </summary>
    /// <param name="path">A path to the configuration file.</param>
    /// <returns>True if the extension matches the encrypted file extension.</returns>
    public static bool IsProtected(string path) => Path.GetExtension(path).Equals(Extension, StringComparison.OrdinalIgnoreCase);

}