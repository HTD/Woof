namespace Woof.LinuxAdmin.Native;

#pragma warning disable CA2101 // Linux libc uses UTF-8 encoding (Ansi marshalling).
#pragma warning disable CS0649 // private struct members are private and unused for a reason - padding.
#pragma warning disable IDE0044 // no reason to make interop fields read only.
#pragma warning disable IDE0051 // private struct members are private and unused for a reason - padding.

/// <summary>
/// Basic Linux libc system calls.
/// </summary>
internal static class Syscall {

    /// <summary>
    /// Changes permissions of a file or directory.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="mode">Mode (raw).</param>
    /// <returns>Zero if successful.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int chmod(string path, uint mode);

    /// <summary>
    /// Changes the owner and group of a file or directory.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="owner">Owner identifier.</param>
    /// <param name="group">Group identifier.</param>
    /// <returns>Zero if successful.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int chown(string path, int owner, int group);

    /// <summary>
    /// Gets the effective user identifier.
    /// </summary>
    /// <returns>User identifier.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int geteuid();

    /// <summary>
    /// Gets the specified group information.
    /// </summary>
    /// <param name="gid">Group identifier.</param>
    /// <returns>Pointer to <see cref="Group"/> structure.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern IntPtr getgrgid(int gid);

    /// <summary>
    /// Gets the specified group information.
    /// </summary>
    /// <param name="name">Group name.</param>
    /// <returns>Pointer to <see cref="Group"/> structure.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern IntPtr getgrnam(string name);

    /// <summary>
    /// Gets the specified user information.
    /// </summary>
    /// <param name="uid">User identifier.</param>
    /// <returns>Pointer to <see cref="Passwd"/> structure.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern IntPtr getpwuid(int uid);

    /// <summary>
    /// Gets the specified user information.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <returns>Pointer to <see cref="Passwd"/> structure.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern IntPtr getpwnam(string name);

    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    /// <returns>User identifier.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int getuid();

    /// <summary>
    /// Changes the owner and group of a symbolic link.
    /// </summary>
    /// <param name="path">Link path.</param>
    /// <param name="owner">Owner identifier.</param>
    /// <param name="group">Group identifier.</param>
    /// <returns>Zero if successful.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int lchown(string path, int owner, int group);

    /// <summary>
    /// Creates a symbolic link.
    /// </summary>
    /// <param name="target">Path to target.</param>
    /// <param name="linkpath">Path for the link.</param>
    /// <returns>Zero if successful.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int symlink(string target, string linkpath);

    /// <summary>
    /// Gets file status (extended).
    /// </summary>
    /// <param name="dirfd">Base directory used. Use -100 for current directory for relative paths.</param>
    /// <param name="path">Path.</param>
    /// <param name="flags">Additional FS synchronization flags.</param>
    /// <param name="mask">Flags to reject in output information.</param>
    /// <param name="data">Data buffer.</param>
    /// <returns>Zero if successful.</returns>
    [DllImport(LIBC, SetLastError = true)]
    internal static extern int statx(int dirfd, string path, int flags, uint mask, out Statx data);

    /// <summary>
    /// "libc" DLL name.
    /// </summary>
    const string LIBC = "c";

    /// <summary>
    /// POSIX group data structure.
    /// </summary>
    internal struct Group {

        /// <summary>
        /// Group name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Group password.
        /// </summary>
        public string Password;

        /// <summary>
        /// Group ID.
        /// </summary>
        public uint Gid;

        /// <summary>
        /// Group members.
        /// </summary>
        public IntPtr Members;

    };

    /// <summary>
    /// POSIX password data structure.
    /// </summary>
    internal struct Passwd {

        /// <summary>
        /// User name.
        /// </summary>
        public string Name;

        /// <summary>
        /// User password.
        /// </summary>
        public string Password;

        /// <summary>
        /// User ID.
        /// </summary>
        public uint Uid;

        /// <summary>
        /// Group ID.
        /// </summary>
        public uint Gid;

        /// <summary>
        /// User information.
        /// </summary>
        public string GECOS;

        /// <summary>
        /// Home directory.
        /// </summary>
        public string Directory;

        /// <summary>
        /// Shell program.
        /// </summary>
        public string Shell;

    }

    /// <summary>
    /// POSIX statx data structure.
    /// </summary>
    internal struct Statx {

        /// <summary>
        /// Mask of bits indicating filled fields.
        /// </summary>
        internal uint Mask;
        /// <summary>
        /// Block size for filesystem I/O.
        /// </summary>
        internal uint BlockSize;
        /// <summary>
        /// Extra file attribute indicators
        /// </summary>
        internal ulong Attributes;
        /// <summary>
        /// Number of hard links.
        /// </summary>
        internal uint HardLinks;
        /// <summary>
        /// User ID of owner.
        /// </summary>
        internal uint Uid;
        /// <summary>
        /// Group ID of owner.
        /// </summary>
        internal uint Gid;
        /// <summary>
        /// File type and mode.
        /// </summary>
        internal ushort Mode;
        private ushort Padding01;
        /// <summary>
        /// Inode number.
        /// </summary>
        internal ulong Inode;
        /// <summary>
        /// Total size in bytes.
        /// </summary>
        internal ulong Size;
        /// <summary>
        /// Number of 512B blocks allocated.
        /// </summary>
        internal ulong Blocks;
        /// <summary>
        /// Mask to show what's supported in <see cref="Attributes"/>.
        /// </summary>
        internal ulong AttributesMask;
        /// <summary>
        /// Last access time.
        /// </summary>
        internal StatxTimeStamp AccessTime;
        /// <summary>
        /// Creation time.
        /// </summary>
        internal StatxTimeStamp CreationTime;
        /// <summary>
        /// Last status change time.
        /// </summary>
        internal StatxTimeStamp StatusChangeTime;
        /// <summary>
        /// Last modification time.
        /// </summary>
        internal StatxTimeStamp LastModificationTime;
        internal uint RDevIdMajor;
        internal uint RDevIdMinor;
        internal uint DevIdMajor;
        internal uint DevIdMinor;
        internal ulong MountId;
        private ulong Padding02;
        private ulong Padding03;
        private ulong Padding04;
        private ulong Padding05;
        private ulong Padding06;
        private ulong Padding07;
        private ulong Padding08;
        private ulong Padding09;
        private ulong Padding10;
        private ulong Padding11;
        private ulong Padding12;
        private ulong Padding13;
        private ulong Padding14;
        private ulong Padding15;
    }

    /// <summary>
    /// Time stamp structure used by statx.
    /// </summary>
    public struct StatxTimeStamp {

        /// <summary>
        /// Seconds since the Epoch (UNIX time).
        /// </summary>
        public long Seconds;

        /// <summary>
        /// Nanoseconds since <see cref="Seconds"/>.
        /// </summary>
        public uint Nanoseconds;

    }

    /// <summary>
    /// Flags used by statx call.
    /// </summary>
    [Flags]
    internal enum StatxFlags : int {

        /// <summary>
        /// Special value used to indicate the *at functions should use the current working directory.
        /// </summary>
        AT_FDCWD = -100,

        /// <summary>
        /// Synchronize as stat.
        /// </summary>
        AT_STATX_SYNC_AS_STAT = 0x0000,

        /// <summary>
        /// Force synchronization.
        /// </summary>
        AT_STATX_FORCE_SYNC = 0x2000,

        /// <summary>
        /// No synchronization.
        /// </summary>
        AT_STATX_DONT_SYNC = 0x4000,

    }

}

#pragma warning restore IDE0051
#pragma warning restore IDE0044
#pragma warning restore CA2101
#pragma warning restore CS0649