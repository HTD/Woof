namespace Woof.LinuxAdmin;

/// <summary>
/// Current user permissions helper. Do not cache the values, current user permissions can change over the program lifetime!
/// </summary>
public static class CurrentUser {

    /// <summary>
    /// Gets a value indicating that the current user has administrative privileges.
    /// </summary>
    /// <remarks>Works on Linux and Windows.</remarks>
    public static bool IsAdmin
        => OS.IsLinux ? IsRoot : OS.IsWindows ? IsWindowsAdmin : throw new PlatformNotSupportedException();

    /// <summary>
    /// Gets a value indicating that the current user has Windows Administrator privileges.
    /// </summary>
    /// <remarks>Do not call on Linux or get the exception.</remarks>
    public static bool IsWindowsAdmin {
        get {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new PlatformNotSupportedException();
            using var currentIdentity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(currentIdentity).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    /// <summary>
    /// Gets a value indicating that the current user has Linux root privileges.
    /// </summary>
    /// <remarks>Do not call on Windows or get the exception.</remarks>
    public static bool IsRoot => Syscall.geteuid() == 0;

    /// <summary>
    /// Gets a value indicating that the current user is a Linux user running sudo.
    /// </summary>
    /// <remarks>When called on Windows returns false obviously.</remarks>
    public static bool IsSudo => Environment.GetEnvironmentVariable("SUDO_UID") is not null;

    /// <summary>
    /// Gets the sudo user information if applicable, null otherwise.
    /// </summary>
    /// <remarks>When called on Windows returns null obviously.</remarks>
    public static UserInfo? BehindSudo {
        get {
            var sudo_uid_str = Environment.GetEnvironmentVariable("SUDO_UID");
            return sudo_uid_str is not null && Int32.TryParse(sudo_uid_str, out int uid)
                ? UserInfo.FromUid(uid)
                : null;
        }
    }

}