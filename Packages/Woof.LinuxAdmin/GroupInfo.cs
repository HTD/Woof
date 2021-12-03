namespace Woof.LinuxAdmin;

/// <summary>
/// Linux group information.
/// </summary>
public sealed class GroupInfo {

    /// <summary>
    /// Gets the group name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the group ID.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the group members.
    /// </summary>
    public string[] Members { get; }

    /// <summary>
    /// Creates <see cref="GroupInfo"/> instance from <see cref="Syscall.Group"/>.
    /// </summary>
    /// <param name="group"><see cref="Syscall.Group"/> structure.</param>
    internal GroupInfo(Syscall.Group group) {
        Name = group.Name;
        Id = (int)group.Gid;
        Members = GetMembers(group.Members).ToArray();
    }

    /// <summary>
    /// Queries the OS for group information by name.
    /// </summary>
    /// <param name="name">Group name.</param>
    /// <returns>Group information or null if doesn't exist.</returns>
    public static GroupInfo? FromName(string name) {
        var result = Syscall.getgrnam(name);
        return result != IntPtr.Zero ? new GroupInfo(Marshal.PtrToStructure<Syscall.Group>(result)) : null;
    }

    /// <summary>
    /// Queries the OS for group information by identifier.
    /// </summary>
    /// <param name="gid">Group identifier.</param>
    /// <returns>Group information or null if doesn't exist.</returns>
    public static GroupInfo? FromGid(int gid) {
        var result = Syscall.getgrgid(gid);
        return result != IntPtr.Zero ? new GroupInfo(Marshal.PtrToStructure<Syscall.Group>(result)) : null;
    }

    /// <summary>
    /// Gets strings from null terminated array of pointers.
    /// </summary>
    /// <param name="members">A pointer to a null terminated array of string pointers.</param>
    /// <returns>Member strings.</returns>
    private static IEnumerable<string> GetMembers(IntPtr members) {
        IntPtr p;
        for (int i = 0; (p = Marshal.ReadIntPtr(members, i * IntPtr.Size)) != IntPtr.Zero; i++)
            yield return Marshal.PtrToStringAnsi(p)!;
    }

    /// <summary>
    /// Returns the group name.
    /// </summary>
    /// <returns>Group name.</returns>
    override public string ToString() => Name;

    /// <summary>
    /// Converts the <see cref="GroupInfo"/> to <see cref="uint"/> identifier.
    /// </summary>
    /// <param name="group"></param>
    public static implicit operator int(GroupInfo group) => group.Id;

    /// <summary>
    /// Converts the <see cref="GroupInfo"/> to <see cref="string   "/> name.
    /// </summary>
    /// <param name="group"></param>
    public static implicit operator string(GroupInfo group) => group.Name;

}