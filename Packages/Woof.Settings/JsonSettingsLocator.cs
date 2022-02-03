namespace Woof.Settings;

/// <summary>
/// Locates the configuration files.
/// </summary>
public class JsonSettingsLocator : ILocator {

    /// <summary>
    /// Gets a value indicating that the entry assembly was built with the Debug configuration.
    /// </summary>
    public static bool IsDebug { get; }
        = Assembly.GetEntryAssembly()!.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);

    /// <summary>
    /// Gets the supported configuration file extensions. Derived class can add its own.
    /// </summary>
    protected virtual IEnumerable<string> Extensions => new[] { ".json" };

    /// <summary>
    /// Gets or sets a value indicating that the user directory will be preferred over the application directory.
    /// </summary>
    public bool PreferUserDirectory { get; set; }

    /// <summary>
    /// Locates a settings file for the specified settings name, or it uses application name if the name is not specified.
    /// </summary>
    /// <param name="name">Settings file base name (no extension).</param>
    /// <returns>A tuple with the full path to the file and a flag indicating whether the file exists.</returns>
    public virtual (string path, bool exists) Locate(string name) {
        if (name is null) throw new ArgumentNullException(nameof(name));
        var programDirectory = Executable.Directory.FullName;
        var userDirectory = UserFiles.UserDirectory.FullName;
        var targets =
            PreferUserDirectory
            ? new[] { userDirectory, programDirectory }
            : new[] { programDirectory, userDirectory };
        var extensions = IsDebug ? GetDebugExtensions() : Extensions;
        var matches = new List<FileInfo>();
        var userTargetExists = false; // introduced not to copy the secondary extension files from program to user directory.
        foreach (var extension in extensions) {
            foreach (var target in targets) { // we search all targets to find the most recent configuration.
                var file = new FileInfo(Path.Combine(target, name + extension));
                if (file.Exists) {
                    if (PreferUserDirectory) {
                        var userTarget = new FileInfo(Path.Combine(userDirectory, file.Name));
                        if (userTarget.Exists) userTargetExists = true;
                        if (file.Directory!.FullName.Equals(programDirectory, StringComparison.OrdinalIgnoreCase)) { // file exists in a program directory, but not user
                            if (!userTargetExists || (userTarget.Exists && file.LastWriteTime > userTarget.LastWriteTime)) { // not matched for primary extension, or there is a newer version
                                if (!userTarget.Directory!.Exists) userTarget.Directory.Create();
                                File.Copy(file.FullName, userTarget.FullName, true);
                                try { File.Delete(file.FullName); } catch { } // this step is optional.
                                file = new FileInfo(userTarget.FullName);
                            }
                        }
                    }
                    matches.Add(file);
                    break;
                }
            }
        }
        var bestMatch = matches.OrderByDescending(m => m.LastWriteTime).FirstOrDefault()?.FullName;
        if (bestMatch is not null) return (bestMatch, true);
        {
            var extension = Extensions.First();
            if (!Directory.Exists(userDirectory)) Directory.CreateDirectory(userDirectory);
            return (Path.Combine(userDirectory, name + extension), false);
        }
    }

    /// <summary>
    /// Finds the primary and secondary location for the new settings file. One of these should be used to save a new file.
    /// </summary>
    /// <param name="name">Settings file base name (no extension).</param>
    /// <returns>
    /// A tuple containing the primary and secondary paths.
    /// The caller can decide whether to use the secondary path if the primary path is not writeable.
    /// </returns>
    public virtual (string primaryPath, string secondaryPath) LocateNew(string name) {
        if (name is null) throw new ArgumentNullException(nameof(name));
        var programDirectory = Executable.Directory.FullName;
        var userDirectory = UserFiles.UserDirectory.FullName;
        var extension = Extensions.First();
        return (
            Path.Combine(PreferUserDirectory ? userDirectory : programDirectory, name + extension),
            Path.Combine(PreferUserDirectory ? programDirectory : userDirectory, name + extension)
        );
    }

    /// <summary>
    /// Gets the extensions for the debug build.
    /// </summary>
    /// <returns>Extensions enumeration with the debug extensions placed first.</returns>
    private IEnumerable<string> GetDebugExtensions() {
        foreach (var extension in Extensions) {
            yield return ".dev" + extension;
            yield return extension;
        }
    }

}
