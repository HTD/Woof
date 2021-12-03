namespace Woof.LinuxAdmin;

/// <summary>
/// Operation system runtime detection helpers.
/// </summary>
public static class OS {

    /// <summary>
    /// Gets a value indicating that the current operating system is Linux.
    /// </summary>
    public static bool IsLinux => _IsLinux ?? (_IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)).Value;

    /// <summary>
    /// Gets a value indicating that the current operating system is Windows.
    /// </summary>
    public static bool IsWindows => _IsWindows ?? (_IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)).Value;

    private static bool? _IsLinux;
    private static bool? _IsWindows;

}