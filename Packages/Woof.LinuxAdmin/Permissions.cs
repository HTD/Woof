namespace Woof.LinuxAdmin;

/// <summary>
/// Represents Linux file system entry permissions.
/// </summary>
public partial class Permissions {

    /// <summary>
    /// Gets a copy of the permissions.
    /// </summary>
    public Permissions Copy => OriginalString is not null ? new Permissions(OriginalString) : new Permissions(Mode);

    /// <summary>
    /// Gets the binary file mode.<br/>
    /// When represented as octal number, last 3 digits contain permissions for user, group and others.
    /// </summary>
    public uint Mode { get; private set; }

    /// <summary>
    /// Gets a value indicating that the value is relative to the existing permissions.<br/>
    /// Relative permissions cannot be used directly. A call to <see cref="Modify(uint)"/> is required.
    /// </summary>
    public bool IsRelative { get; private set; }

    /// <summary>
    /// Returns the permissions in ls format.
    /// </summary>
    public string LsString {
        get {
            var ls = new char[10];
            const uint d = (uint)Bits.Directory;
            const uint r = (uint)Bits.Read;
            const uint w = (uint)Bits.Write;
            const uint x = (uint)Bits.Execute;
            ls[0] = (Mode & d) > 0 ? ls[0] = 'd' : '-';
            ls[1] = (Mode & r << 6) > 0 ? ls[1] = 'r' : '-';
            ls[2] = (Mode & w << 6) > 0 ? ls[2] = 'w' : '-';
            ls[3] = (Mode & x << 6) > 0 ? ls[3] = 'x' : '-';
            ls[4] = (Mode & r << 3) > 0 ? ls[4] = 'r' : '-';
            ls[5] = (Mode & w << 3) > 0 ? ls[5] = 'w' : '-';
            ls[6] = (Mode & x << 3) > 0 ? ls[6] = 'x' : '-';
            ls[7] = (Mode & r) > 0 ? ls[7] = 'r' : '-';
            ls[8] = (Mode & w) > 0 ? ls[8] = 'w' : '-';
            ls[9] = (Mode & x) > 0 ? ls[9] = 'x' : '-';
            return ls.AsSpan().ToString();
        }
    }

    /// <summary>
    /// Creates Linux file system entry permissions from string, like "660" or "+x".
    /// </summary>
    /// <param name="value">Permission string.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Argument is not a valid permission string.</exception>
    public Permissions(string value) {
        if (value is null) throw new ArgumentNullException(nameof(value));
        OriginalString = value;
        if (RxOctalString.IsMatch(value)) {
            Mode = Convert.ToUInt32(value, 8);
            return;
        }
        IsRelative = true;
    }

    /// <summary>
    /// Creates Linux file system entry permissions from <see cref="uint"/>.
    /// </summary>
    /// <param name="mode"><see cref="uint"/> mode.</param>
    public Permissions(uint mode) => Mode = mode;

    /// <summary>
    /// Checks if the current value matches specified target and permissions bits.
    /// </summary>
    /// <param name="target">Any combination of the <see cref="Target"/> flags.</param>
    /// <param name="bits">One of the permissions <see cref="Bits"/> flags.</param>
    /// <returns>True if any of the permissions bits are set for the specified target.</returns>
    public bool IsMatch(Target target, Bits bits) => (Mode & GetEffectiveBits(target, bits)) > 0;

    /// <summary>
    /// Modifies original file system entry permissions with the <see cref="OriginalString"/>.
    /// </summary>
    /// <param name="mode"><see cref="uint"/> mode.</param>
    /// <exception cref="InvalidOperationException">Original string is not a valid permissions string.</exception>
    /// <returns>This modified, absolute permissions.</returns>
    public Permissions Modify(uint mode) {
        if (!IsRelative) throw new InvalidOperationException(EAbsolute);
        Mode = mode;
        foreach (var partString in RxSplitRelative.Split(OriginalString)) {
            var (target, op, bits) = ParsePart(partString);
            switch (op) {
                case '+': Mode |= GetEffectiveBits(target, bits); break;
                case '-': Mode &= GetEffectiveBits(target, bits, invert: true); break;
                case '=': Mode &= ~0xfffu; Mode |= GetEffectiveBits(target, bits); break;
                default: throw new InvalidOperationException(EInvalid);
            }
        }
        IsRelative = false;
        return this;
    }

    /// <summary>
    /// Gets the hash code depending on <see cref="Mode"/> and whether the value is relative.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => HashCode.Combine(Mode, IsRelative);

    /// <summary>
    /// Tests for equality with another object.
    /// </summary>
    /// <param name="obj">The other object.</param>
    /// <returns>True if equals.</returns>
    public override bool Equals(object? obj)
        => obj is Permissions p && p.Mode == Mode && p.IsRelative == IsRelative;

    /// <summary>
    /// Equality test.
    /// </summary>
    /// <param name="a">Left side.</param>
    /// <param name="b">Right side.</param>
    /// <returns>True if equal.</returns>
    public static bool operator ==(Permissions a, Permissions b)
        => a is null && b is null || a is not null && a.Equals(b) || b is not null && b.Equals(a);

    /// <summary>
    /// Inequality test.
    /// </summary>
    /// <param name="a">Left side.</param>
    /// <param name="b">Right side.</param>
    /// <returns>True if not equal.</returns>
    public static bool operator !=(Permissions a, Permissions b)
        => !(a is null && b is null) || a is not null && !a.Equals(b) || b is not null && !b.Equals(a);

    /// <summary>
    /// Returns original string used to create the instance, or the octal value.
    /// </summary>
    /// <returns>Original string or the octal value.</returns>
    public override string ToString() => IsRelative ? OriginalString : Convert.ToString(Mode, 8);

    #region Conversions

    /// <summary>
    /// Converts to <see cref="uint"/>.
    /// </summary>
    /// <param name="permissions">Value to convert.</param>
    public static implicit operator uint(Permissions permissions) => permissions.Mode;

    /// <summary>
    /// Converts from <see cref="uint"/>.
    /// </summary>
    /// <param name="mode">Value to convert from.</param>
    public static implicit operator Permissions(uint mode) => new(mode);

    /// <summary>
    /// Converts to <see cref="string"/>.
    /// </summary>
    /// <param name="permissions">Value to convert.</param>
    public static implicit operator string(Permissions permissions) => permissions.ToString();

    /// <summary>
    /// Converts from <see cref="string"/>.
    /// </summary>
    /// <param name="value">Value to convert from.</param>
    public static implicit operator Permissions(string value) => new(value);

    #endregion

    #region Implementation details

    /// <summary>
    /// Gets the bit masks for part modification
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="bits">Bits selected.</param>
    /// <param name="invert">Invert result to pass all except set.</param>
    /// <returns>Effective bits to set or pass.</returns>
    private uint GetEffectiveBits(Target target, Bits bits, bool invert = false) {
        var tmask = (uint)target;
        var umask = (uint)bits & 0xfffu;
        if (!invert) tmask &= ~0u ^ 0b001001001u | (Mode & 0b100100100u) >> 2; // don't allow adding execute on unreadable.
        if (bits.HasFlag(Bits.SpecialExecute) && IsDirOrExecutable) umask |= 1;
        var result = tmask & (umask << 6 | umask << 3 | umask);
        return invert ? ~result : result;
    }

    /// <summary>
    /// Gets a value indicating that the current mask matches a directory or an executable file.
    /// </summary>
    private bool IsDirOrExecutable => (Mode & 0x4049 ) > 0; // (040111) - directory or any executable bit set

    /// <summary>
    /// Parses a permissions string part.
    /// </summary>
    /// <param name="partString">Permissions string part.</param>
    /// <returns>A tuple containing target, operation and bits to change.</returns>
    /// <exception cref="ArgumentException"><paramref name="partString"/> is not a valid permissions string part.</exception>
    private static (Target target, char op, Bits bits) ParsePart(string partString) {
        var m = RxRelativePart.Match(partString);
        if (!m.Success || m.Groups.Count != 4) throw new ArgumentException(EInvalid, nameof(partString));
        string ts = m.Groups[1].Value;
        char o = m.Groups[2].Value[0];
        string bs = m.Groups[3].Value;
        Target t = 0;
        Bits b = 0;
        StringComparison oc = StringComparison.Ordinal;
        if (ts.Length < 1) t = Target.User | Target.Group | Target.Other;
        else {
            if (ts.Contains('u', oc)) t |= Target.User;
            if (ts.Contains('g', oc)) t |= Target.Group;
            if (ts.Contains('o', oc)) t |= Target.Other;
            if (ts.Contains('a', oc)) t |= Target.User | Target.Group | Target.Other;
        }
        if (bs.Length < 1) throw new ArgumentException(EInvalid, nameof(partString));
        if (bs.Contains('r')) b |= Bits.Read;
        if (bs.Contains('w')) b |= Bits.Write;
        if (bs.Contains('x')) b |= Bits.Execute;
        if (bs.Contains('X')) b |= Bits.SpecialExecute;
        if (bs.Contains('s')) b |= Bits.Stictky;
        return (t, o, b);
    }

    /// <summary>
    /// Target bit masks. The <see cref="uint"/> values represent all permisions bits for user, group and others.
    /// </summary>
    [Flags]
    public enum Target : uint {

        /// <summary>
        /// Access for the users outside the owner group and user.
        /// </summary>
        Other = 7,  // (00007)

        /// <summary>
        /// Access for the owner group member.
        /// </summary>
        Group = 56, // (00070)

        /// <summary>
        /// Access for the owner only.
        /// </summary>
        User = 448  // (00700)

    }

    /// <summary>
    /// Permissions bits.
    /// </summary>
    [Flags]
    public enum Bits : uint {

        /// <summary>
        /// For files allows executing. For directoring allows searching.
        /// </summary>
        Execute = 1,

        /// <summary>
        /// Write access allowed.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Read access allowed.
        /// </summary>
        Read = 4,

        /// <summary>
        /// Unlnk and rename for root and owner only.
        /// </summary>
        Stictky = 512,         // (001000)

        /// <summary>
        /// Works like <see cref="Execute"/>, but applied only to directories or files with any <see cref="Execute"/> bit set.
        /// </summary>
        SpecialExecute = 0x1000, // (010000) - the value is not a part of Linux structure and is cleared internally.

        /// <summary>
        /// The entry is a directory.
        /// </summary>
        Directory = 0x4000

    }

    #region Private data

    private readonly string OriginalString = "";
    private static readonly Regex RxOctalString = RxOctalStringCT();
    private static readonly Regex RxRelativePart = RxRelativePartCT();
    private static readonly Regex RxSplitRelative = RxSplitRelativeCT();
    const string EInvalid = "Invalid permissions string";
    const string EAbsolute = "Cannot modify absolute permissions";

    [GeneratedRegex("^[0-7]{3,7}$", RegexOptions.Compiled)]
    private static partial Regex RxOctalStringCT();

    [GeneratedRegex("^([ugoa]*)([=+-])([rwxXs]+)$", RegexOptions.Compiled)]
    private static partial Regex RxRelativePartCT();

    [GeneratedRegex("[, ]", RegexOptions.Compiled)]
    private static partial Regex RxSplitRelativeCT();

    #endregion

    #endregion

}
