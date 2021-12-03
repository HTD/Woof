namespace Woof.LinuxAdmin;

/// <summary>
/// Linux utilites using either libc or shell.
/// </summary>
public static class Linux {

    /// <summary>
    /// Gets the current process user information.
    /// </summary>
    public static UserInfo CurrentProcessUser => UserInfo.CurrentProcessUser;

    /// <summary>
    /// Gets the effective user identifier.
    /// </summary>
    public static int EUid => (int)Syscall.geteuid();

    /// <summary>
    /// Gets the home directory of the current user, or the user who run the program with sudo.
    /// </summary>
    public static string? HomeDirectory => CurrentUser.BehindSudo is UserInfo sudoUser ? sudoUser.Directory : CurrentProcessUser.Directory;

    /// <summary>
    /// Gets the current user identifier. Most of the time it returns the effective UID anyway.
    /// </summary>
    public static int Uid => (int)Syscall.getuid();

    /// <summary>
    /// Adds a system user using shell useradd command.
    /// </summary>
    /// <param name="user">User name. Group with the same name will be created.</param>
    /// <param name="directory">Base directory.</param>
    /// <returns>A <see cref="ValueTask"/> returning true if the user was added.</returns>
    public static async ValueTask<bool> AddSystemUserAsync(string user, string directory) {
        if (UserExists(user)) return false;
        try {
            await new ShellCommand($"useradd -b {directory} -MU -r {user}").ExecVoidAsync();
            return true;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Adds a system user using shell useradd command.
    /// </summary>
    /// <param name="user">User name. Group with the same name will be created.</param>
    /// <param name="group">Group name. It will be created if not exists.</param>
    /// <param name="directory">Base directory.</param>
    /// <returns>A <see cref="ValueTask"/> returning true if the user was added.</returns>
    public static async ValueTask<bool> AddSystemUserAsync(string user, string group, string directory) {
        if (UserExists(user)) return false;
        try {
            if (user == group)
                await new ShellCommand($"useradd -b {directory} -MU -r {user}").ExecVoidAsync();
            else {
                var groupInfo = GroupInfo.FromName(group);
                if (groupInfo is null && !await AddSystemGroupAsync(group)) return false;
                await new ShellCommand($"useradd -b {directory} -g {group} -MN -r {user}").ExecVoidAsync();
            }
            return true;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Adds a system group using shell groupadd command.
    /// </summary>
    /// <param name="group">Group name.</param>
    /// <returns>A <see cref="ValueTask"/> returning true if the group was added.</returns>
    public static async ValueTask<bool> AddSystemGroupAsync(string group) {
        if (GroupExists(group)) return false;
        try {
            await new ShellCommand($"groupadd -r {group}").ExecVoidAsync();
            return true;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Creates a system directory with permission set to the specified user and group.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">Owner name.</param>
    /// <param name="group">Owning group name.</param>
    /// <returns>True if successful or the specified directory already exists with appropriate permissions.</returns>
    public static bool AddSystemDirectory(string path, string user, string group) {
        var userInfo = UserInfo.FromName(user);
        var groupInfo = GroupInfo.FromName(group);
        if (userInfo is null || groupInfo is null) return false;
        try {
            Directory.CreateDirectory(path);
            return Syscall.chown(path, userInfo.Uid, groupInfo.Id) == 0;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Changes permissions of a file or directory.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="permissions">Permissions (automatic conversion from <see cref="string"/> and <see cref="uint"/> available).</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="permissions"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="permissions"/> is not a valid permissions string.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="permissions"/> is not a valid permission string or stat call failed.</exception>
    public static bool Chmod(string path, Permissions permissions) {
        if (permissions.IsRelative) {
            var stat = Stat(path);
            if (stat is null) return false;
            permissions = permissions.Copy.Modify(stat.Permissions);
        }
        return Syscall.chmod(path, permissions.Mode) == 0;
    }

    /// <summary>
    /// Changes permissions of all file system entries in specified directory.
    /// </summary>
    /// <param name="path">Directory.</param>
    /// <param name="permissions">Permissions (automatic conversion from <see cref="string"/> and <see cref="uint"/> available).</param>
    /// <param name="pattern">Search pattern.</param>
    /// <returns>True if successful for ALL items.</returns>
    public static bool ChmodR(string path, Permissions permissions, string pattern = "*") {
        if (!new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory)) return Chmod(path, permissions);
        var ok = Chmod(path, permissions);
        foreach (var entry in Directory.EnumerateFileSystemEntries(path, pattern, SearchOption.AllDirectories))
            ok = ok && Chmod(entry, permissions);
        return ok;
    }

    /// <summary>
    /// Changes the owner and group of a file system entry.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="uid">User identifier.</param>
    /// <param name="gid">Group identifier.</param>
    /// <returns>True if successful.</returns>
    public static bool Chown(string path, int uid, int gid) => Syscall.chown(path, uid, gid) == 0;

    /// <summary>
    /// Changes the owner and group of a file system entry.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User name.</param>
    /// <param name="group">Group name.</param>
    /// <returns>True if successful.</returns>
    public static bool Chown(string path, string user, string group) {
        var u = UserInfo.FromName(user);
        if (u is null) return false;
        var g = GroupInfo.FromName(group);
        return g is not null && Syscall.chown(path, u.Uid, g.Id) == 0;
    }

    /// <summary>
    /// Changes the owner and group of a file system entry.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User information containing the group to set.</param>
    /// <returns>True if successful.</returns>
    public static bool Chown(string path, UserInfo user) => Syscall.chown(path, user.Uid, user.Gid) == 0;

    /// <summary>
    /// Changes the owner and group of a file system entry.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User information.</param>
    /// <param name="group">Group information.</param>
    /// <returns>True if successful.</returns>
    public static bool Chown(string path, UserInfo user, GroupInfo group) => Syscall.chown(path, user.Uid, group.Id) == 0;

    /// <summary>
    /// Changes the owner and group of all file system entries in the specified path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="uid">User identifier.</param>
    /// <param name="gid">Group identifier.</param>
    /// <param name="pattern">Search pattern.</param>
    /// <returns>True if successful for ALL items.</returns>
    public static bool ChownR(string path, int uid, int gid, string pattern = "*") {
        if (!new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory)) return Syscall.chown(path, uid, gid) == 0;
        var ok = Syscall.chown(path, uid, gid) == 0;
        foreach (var entry in Directory.EnumerateFileSystemEntries(path, pattern, SearchOption.AllDirectories))
            ok = ok && Syscall.chown(entry, uid, gid) == 0;
        return ok;
    }

    /// <summary>
    /// Changes the owner and group of all file system entries in the specified path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User name.</param>
    /// <param name="group">Group name.</param>
    /// <param name="pattern">Search pattern.</param>
    /// <returns>True if successful for ALL items.</returns>
    public static bool ChownR(string path, string user, string group, string pattern = "*") {
        var uid = UserInfo.FromName(user);
        if (uid is null) return false;
        var gid = GroupInfo.FromName(group);
        return gid is not null && ChownR(path, uid.Uid, gid.Id, pattern);
    }

    /// <summary>
    /// Changes the owner and group of all file system entries in the specified path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User information containing the group to set.</param>
    /// <param name="pattern">Search pattern.</param>
    /// <returns>True if successful for ALL items.</returns>
    public static bool ChownR(string path, UserInfo user, string pattern = "*") => ChownR(path, user.Uid, user.Gid, pattern);

    /// <summary>
    /// Changes the owner and group of all file system entries in the specified path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User information.</param>
    /// <param name="group">Group information.</param>
    /// <param name="pattern">Search pattern.</param>
    /// <returns>True if successful for ALL items.</returns>
    public static bool ChownR(string path, UserInfo user, GroupInfo group, string pattern = "*") => ChownR(path, user.Uid, group.Id, pattern);

    /// <summary>
    /// Changes the owner and group of a symbolic link.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="uid">User identifier.</param>
    /// <param name="gid">Group identifier.</param>
    /// <returns>True if successful.</returns>
    public static bool Lchown(string path, int uid, int gid) => Syscall.lchown(path, uid, gid) == 0;

    /// <summary>
    /// Changes the owner and group of a symbolic link.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User name.</param>
    /// <param name="group">Group name.</param>
    /// <returns>True if successful.</returns>
    public static bool Lchown(string path, string user, string group) {
        var u = UserInfo.FromName(user);
        if (u is null) return false;
        var g = GroupInfo.FromName(group);
        return g is not null && Syscall.lchown(path, u.Uid, g.Id) == 0;
    }

    /// <summary>
    /// Changes the owner and group of a symbolic link.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User information containing the group to set.</param>
    /// <returns>True if successful.</returns>
    public static bool Lchown(string path, UserInfo user) => Syscall.lchown(path, user.Uid, user.Gid) == 0;

    /// <summary>
    /// Changes the owner and group of a symbolic link.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="user">User information.</param>
    /// <param name="group">Group information.</param>
    /// <returns>True if successful.</returns>
    public static bool Lchown(string path, UserInfo user, GroupInfo group) => Syscall.lchown(path, user.Uid, group.Id) == 0;

    /// <summary>
    /// Checks if the specified user exists.
    /// </summary>
    /// <param name="uid">User identifier.</param>
    /// <returns>True if the user exists.</returns>
    public static bool UserExists(int uid) => UserInfo.FromUid(uid) is not null;

    /// <summary>
    /// Checks if the specified user name exists.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <returns>True if the user exists.</returns>
    public static bool UserExists(string name) => UserInfo.FromName(name) is not null;

    /// <summary>
    /// Checs if the specified group exists.
    /// </summary>
    /// <param name="gid">Group name.</param>
    /// <returns>True if the group exists.</returns>
    public static bool GroupExists(int gid) => GroupInfo.FromGid(gid) is not null;

    /// <summary>
    /// Checs if the specified group name exists.
    /// </summary>
    /// <param name="name">Group name.</param>
    /// <returns>True if the group exists.</returns>
    public static bool GroupExists(string name) => GroupInfo.FromName(name) is not null;

    /// <summary>
    /// Gets information about a file system entry.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns>Detailed information about the entry or null if the path is not accessible.</returns>
    public static StatInfo? Stat(string path)
        => Syscall.statx(
            (int)Syscall.StatxFlags.AT_FDCWD,
            path,
            (int)Syscall.StatxFlags.AT_STATX_SYNC_AS_STAT,
            0,
            out var stx
        ) != 0 ? null : new StatInfo(stx);

    /// <summary>
    /// Creates a symbolic link.
    /// </summary>
    /// <param name="target">Path to target.</param>
    /// <param name="path">Path for the link.</param>
    /// <returns>True if successful.</returns>
    public static bool Symlink(string target, string path) => Syscall.symlink(target, path) == 0;

    /// <summary>
    /// Resolves the path containing "~" by replacing it with absolute home directory location.<br/>
    /// When used with sudo the "~' directory will still be resolved to the home directory of the user who run sudo.
    /// </summary>
    /// <param name="path">Path containing "~" reference.</param>
    /// <returns>Absolute path.</returns>
    public static string ResolveUserPath(string path) => path.Replace("~", HomeDirectory);

}