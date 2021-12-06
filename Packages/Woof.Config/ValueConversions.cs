namespace Woof.Config;

/// <summary>
/// Defines from string and to string conversions for types.
/// </summary>
public class ValueConversions : Dictionary<Type, (ValueParser Parse, ValueToString GetString, bool IsQuoted)> {

    /// <summary>
    /// Gets the value conversions for the basic system types.
    /// </summary>
    /// <remarks>
    /// <see cref="TimeSpan"/> is expressed as a floating point number of seconds.<br/>
    /// <see cref="DateTime"/>, <see cref="DateOnly"/> and <see cref="TimeOnly"/> is expressed in ISO8601 format.<br/>
    /// <see cref="T:byte[]"/> is expressed as Base64 encoded string.
    /// </remarks>
    public static ValueConversions Default { get; } = new() {
        [typeof(string)] = (value => value, value => (string)value, true),
        [typeof(bool)] = (value => bool.Parse(value), value => ((bool)value).ToString(), false),
        [typeof(int)] = (value => int.Parse(value, N), value => ((int)value).ToString(N), false),
        [typeof(double)] = (value => double.Parse(value, N), value => ((double)value).ToString(N), false),
        [typeof(decimal)] = (value => decimal.Parse(value, N), value => ((decimal)value).ToString(N), false),
        [typeof(DateTime)] = (value => DateTime.Parse(value, N), value => ((DateTime)value).ToString("s"), true),
        [typeof(TimeSpan)] = (value => TimeSpan.FromSeconds(double.Parse(value, N)), value => ((TimeSpan)value).TotalSeconds.ToString(N), false),
        [typeof(byte[])] = (value => Convert.FromBase64String(value), value => Convert.ToBase64String((byte[])value), true),
        [typeof(float)] = (value => float.Parse(value, N), value => ((float)value).ToString(N), false),
        [typeof(byte)] = (value => byte.Parse(value, N), value => ((byte)value).ToString(N), false),
        [typeof(short)] = (value => short.Parse(value, N), value => ((short)value).ToString(N), false),
        [typeof(long)] = (value => long.Parse(value, N), value => ((long)value).ToString(N), false),
        [typeof(sbyte)] = (value => sbyte.Parse(value, N), value => ((sbyte)value).ToString(N), false),
        [typeof(ushort)] = (value => ushort.Parse(value, N), value => ((ushort)value).ToString(N), false),
        [typeof(uint)] = (value => uint.Parse(value, N), value => ((uint)value).ToString(N), false),
        [typeof(ulong)] = (value => ulong.Parse(value, N), value => ((ulong)value).ToString(N), false),
        [typeof(DateOnly)] = (value => DateOnly.Parse(value, N), value => ((DateOnly)value).ToString("yyyy-MM-dd"), true),
        [typeof(TimeOnly)] = (value => TimeOnly.Parse(value, N), value => ((TimeOnly)value).ToString("HH:mm:ss.fff"), true),
        [typeof(Uri)] = (value => new Uri(value), value => ((Uri)value).ToString(), true),
        [typeof(FileInfo)] = (value => new FileInfo(value), value => ((FileInfo)value).ToString(), true),
        [typeof(DirectoryInfo)] = (value => new DirectoryInfo(value), value => ((DirectoryInfo)value).ToString(), true),
        [typeof(Guid)] = (value => new Guid(value), value => ((Guid)value).ToString(), true),
    };

    /// <summary>
    /// Invariant culture format provider for JSON compatible conversions.
    /// </summary>
    private static readonly IFormatProvider N = CultureInfo.InvariantCulture;

}

/// <summary>
/// Defines a function that parses a string value to a value object.
/// </summary>
/// <param name="value">Value string.</param>
/// <returns>Value object.</returns>
public delegate object ValueParser(string value);

/// <summary>
/// Defines a function that converts a value object to a string.
/// </summary>
/// <param name="value">Value object.</param>
/// <returns>Value string.</returns>
public delegate string ValueToString(object value);