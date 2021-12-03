namespace Woof.LinuxAdmin;

/// <summary>
/// Contains information about Linux file system entry.
/// </summary>
public class StatInfo {

    /// <summary>
    /// Gets the owner user identifier.
    /// </summary>
    public uint Uid { get; }

    /// <summary>
    /// Gets the owner group identifier.
    /// </summary>
    public uint Gid { get; }

    /// <summary>
    /// Gets the entry permissions.
    /// </summary>
    public Permissions Permissions { get; }

    /// <summary>
    /// Gets the last access time.
    /// </summary>
    public LinuxTime AccessTime { get; }

    /// <summary>
    /// Gets the creation time.
    /// </summary>
    public LinuxTime CreationTime { get; }

    /// <summary>
    /// Gets the last status change time.
    /// </summary>
    public LinuxTime StatusChangeTime { get; }

    /// <summary>
    /// Gets the last modification time.
    /// </summary>
    public LinuxTime LastModificationTime { get; }

    /// <summary>
    /// Creates the manged structure from statx returned data.
    /// </summary>
    /// <param name="stx">The data structure returned from statx call.</param>
    internal StatInfo(Syscall.Statx stx) {
        Uid = stx.Uid;
        Gid = stx.Gid;
        Permissions = new Permissions(stx.Mode);
        AccessTime = new LinuxTime(stx.AccessTime);
        CreationTime = new LinuxTime(stx.CreationTime);
        StatusChangeTime = new LinuxTime(stx.StatusChangeTime);
        LastModificationTime = new LinuxTime(stx.LastModificationTime);
    }

}