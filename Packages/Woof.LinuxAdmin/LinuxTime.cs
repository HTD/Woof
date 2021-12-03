namespace Woof.LinuxAdmin;

/// <summary>
/// Represents a Linux time stamp with nanoseconds.
/// </summary>
public struct LinuxTime : IEquatable<LinuxTime>, IComparable<LinuxTime> {

    /// <summary>
    /// Contains the number of seconds since the <see cref="DateTimeOffset.UnixEpoch"/>.
    /// </summary>
    public long Seconds;

    /// <summary>
    /// Contains the number of nanoseconds since the <see cref="Seconds"/> value.
    /// </summary>
    public uint Nanoseconds;

    /// <summary>
    /// Gets the Linux time as .NET <see cref="DateTime"/>.
    /// </summary>
    public DateTime AsDateTime => DateTime.UnixEpoch.AddSeconds(Seconds + Nano * Nanoseconds).ToLocalTime();

    /// <summary>
    /// Creates a Linux time stamp from .NET time.
    /// </summary>
    /// <param name="time">.NET time.</param>
    public LinuxTime(DateTime time) {
        double seconds = (time.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
        Seconds = (int)seconds;
        Nanoseconds = (uint)((Seconds - seconds) * NanoInv);
    }

    /// <summary>
    /// Creates a Linux time stamp from libc statx returned structure.
    /// </summary>
    /// <param name="time"></param>
    internal LinuxTime(Syscall.StatxTimeStamp time) {
        Seconds = time.Seconds;
        Nanoseconds = time.Nanoseconds;
    }

    /// <summary>
    /// Converts LinuxTime values to .NET <see cref="DateTime"/> local time.
    /// </summary>
    /// <param name="time">Linux time stamp.</param>
    public static implicit operator DateTime(LinuxTime time) => time.AsDateTime;

    /// <summary>
    /// Converts the local .NET <see cref="DateTime"/> to <see cref="LinuxTime"/>.
    /// </summary>
    /// <param name="time">Time.</param>
    public static implicit operator LinuxTime(DateTime time) => new(time);

    /// <summary>
    /// Equality test.
    /// </summary>
    /// <param name="other">Compared time stamp.</param>
    /// <returns>True if equal.</returns>
    public bool Equals(LinuxTime other) => Seconds == other.Seconds && Nanoseconds == other.Nanoseconds;

    /// <summary>
    /// Equality test.
    /// </summary>
    /// <param name="obj">Compared object.</param>
    /// <returns>True if equal.</returns>
    public override bool Equals(object? obj) => obj is LinuxTime t && t.Equals(this);

    /// <summary>
    /// Compares two Linux time instances.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns>-1 if this comes before the other, zero for equal values, if this comes after the other.</returns>
    public int CompareTo(LinuxTime other) {
        double time = Seconds + Nano * Nanoseconds;
        double otherTime = other.Seconds + Nano * Nanoseconds;
        return time < otherTime ? -1 : time > otherTime ? 1 : 0;
    }

    /// <summary>
    /// Gets the hash code for the structure.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(Seconds, Nanoseconds);

    /// <summary>
    /// Tests for equality.
    /// </summary>
    /// <param name="left">Left side.</param>
    /// <param name="right">Right side.</param>
    /// <returns>True if equal.</returns>
    public static bool operator ==(LinuxTime left, LinuxTime right) => left.Equals(right);

    /// <summary>
    /// Tests for inequality.
    /// </summary>
    /// <param name="left">Left side.</param>
    /// <param name="right">Right side.</param>
    /// <returns>True if inequal.</returns>
    public static bool operator !=(LinuxTime left, LinuxTime right) => !(left == right);

    /// <summary>
    /// Tests for left less than right.
    /// </summary>
    /// <param name="left">Left side.</param>
    /// <param name="right">Right side.</param>
    /// <returns>True if left less than right.</returns>
    public static bool operator <(LinuxTime left, LinuxTime right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Tests for left less or equal right.
    /// </summary>
    /// <param name="left">Left side.</param>
    /// <param name="right">Right side.</param>
    /// <returns>True if left less or equal right.</returns>
    public static bool operator <=(LinuxTime left, LinuxTime right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Tests for left greater than right.
    /// </summary>
    /// <param name="left">Left side.</param>
    /// <param name="right">Right side.</param>
    /// <returns>True if left greater than right.</returns>
    public static bool operator >(LinuxTime left, LinuxTime right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Tests for left greater or equal right.
    /// </summary>
    /// <param name="left">Left side.</param>
    /// <param name="right">Right side.</param>
    /// <returns>True if left greater or equal right.</returns>
    public static bool operator >=(LinuxTime left, LinuxTime right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Nano multiplier.
    /// </summary>
    const double Nano = 0.000000001;

    /// <summary>
    /// Nano multiplier inverse.
    /// </summary>
    const double NanoInv = 1000000000;

}