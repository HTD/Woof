namespace Woof.LinuxAdmin;

/// <summary>
/// Linux user information.
/// </summary>
public class UserInfo {

    /// <summary>
    /// Gets the user name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public int Uid { get; }

    /// <summary>
    /// Gets the group ID.
    /// </summary>
    public int Gid { get; }

    /// <summary>
    /// Gets the home directory.
    /// </summary>
    public string? Directory { get; }

    /// <summary>
    /// Gets the configured shell program.
    /// </summary>
    public string? Shell { get; }

    /// <summary>
    /// Gets the current process user information.
    /// </summary>
    public static UserInfo CurrentProcessUser => FromUid(Syscall.geteuid())!;

    /// <summary>
    /// Gets the current main executable owner user information.
    /// </summary>
    public static UserInfo CurrentExecutableOwner => FromUid((int)Linux.Stat(Application.Path)!.Uid)!;

    /// <summary>
    /// Creates the <see cref="UserInfo"/> instance from <see cref="Syscall.Passwd"/>.
    /// </summary>
    /// <param name="passwd"><see cref="Syscall.Passwd"/> structure.</param>
    internal UserInfo(Syscall.Passwd passwd) {
        Name = passwd.Name;
        Uid = (int)passwd.Uid;
        Gid = (int)passwd.Gid;
        Directory = passwd.Directory;
        Shell = passwd.Shell;
    }

    /// <summary>
    /// Queries the OS for user information by name.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <returns>User information or null if doesn't exist.</returns>
    public static UserInfo? FromName(string name) {
        var result = Syscall.getpwnam(name);
        return result != IntPtr.Zero ? new UserInfo(Marshal.PtrToStructure<Syscall.Passwd>(result)) : null;
    }

    /// <summary>
    /// Queries the OS for user information by identifier.
    /// </summary>
    /// <param name="uid">User identifier.</param>
    /// <returns>User information or null if doesn't exist.</returns>
    public static UserInfo? FromUid(int uid) {
        var result = Syscall.getpwuid(uid);
        return result != IntPtr.Zero ? new UserInfo(Marshal.PtrToStructure<Syscall.Passwd>(result)) : null;
    }

    /// <summary>
    /// Returns the user name.
    /// </summary>
    /// <returns>User name.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Converts the <see cref="UserInfo"/> type to <see cref="uint"/> identifier.
    /// </summary>
    /// <param name="info"></param>
    public static implicit operator int(UserInfo info) => info.Uid;

    /// <summary>
    /// Converts the <see cref="UserInfo"/> type to <see cref="string"/> name.
    /// </summary>
    /// <param name="info"></param>
    public static implicit operator string(UserInfo info) => info.Name;

}