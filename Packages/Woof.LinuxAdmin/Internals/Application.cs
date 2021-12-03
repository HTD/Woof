namespace Woof.LinuxAdmin.Internals;

/// <summary>
/// Gets the information about the current application file and directory.
/// </summary>
internal static class Application {

    /// <summary>
    /// Gets the application base directory.
    /// </summary>
    public static readonly string Directory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase!;

    /// <summary>
    /// Gets the application executable file path.
    /// </summary>
    public static readonly string Path = Process.GetCurrentProcess()?.MainModule?.FileName!;

    /// <summary>
    /// Gets the application executable file information.
    /// </summary>
    public static readonly FileInfo Info = new(Path);

    /// <summary>
    /// Gets the application executable file name without the extension.
    /// </summary>
    public static readonly string Name = System.IO.Path.GetFileNameWithoutExtension(Info.Name);

}