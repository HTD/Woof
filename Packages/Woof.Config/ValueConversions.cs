namespace Woof.Config;

/// <summary>
/// Defines from string and to string conversions for types.
/// </summary>
public class ValueConversions : Dictionary<Type, (ValueParser Parse, ValueToString GetString)> {

    /// <summary>
    /// Gets the value conversions for the basic system types.
    /// </summary>
    /// <remarks>
    /// <see cref="TimeSpan"/> is expressed as a floating point number of seconds.<br/>
    /// <see cref="DateTime"/>, <see cref="DateOnly"/> and <see cref="TimeOnly"/> is expressed in ISO8601 format.<br/>
    /// <see cref="T:byte[]"/> is expressed as Base64 encoded string.
    /// </remarks>
    public static ValueConversions Default { get; } = new() {
        [typeof(string)] = (value => value, value => (string)value),
        [typeof(bool)] = (value => bool.Parse(value), value => ((bool)value).ToString()),
        [typeof(int)] = (value => int.Parse(value, N), value => ((bool)value).ToString(N)),
        [typeof(double)] = (value => double.Parse(value, N), value => ((double)value).ToString(N)),
        [typeof(decimal)] = (value => decimal.Parse(value, N), value => ((decimal)value).ToString(N)),
        [typeof(DateTime)] = (value => DateTime.Parse(value, N), value => ((DateTime)value).ToString("s")),
        [typeof(TimeSpan)] = (value => TimeSpan.FromSeconds(double.Parse(value, N)), value => ((TimeSpan)value).TotalSeconds.ToString(N)),
        [typeof(byte[])] = (value => Convert.FromBase64String(value), value => Convert.ToBase64String((byte[])value)),
        [typeof(float)] = (value => float.Parse(value, N), value => ((float)value).ToString(N)),
        [typeof(byte)] = (value => byte.Parse(value, N), value => ((byte)value).ToString(N)),
        [typeof(short)] = (value => short.Parse(value, N), value => ((short)value).ToString(N)),
        [typeof(long)] = (value => long.Parse(value, N), value => ((long)value).ToString(N)),
        [typeof(sbyte)] = (value => sbyte.Parse(value, N), value => ((sbyte)value).ToString(N)),
        [typeof(ushort)] = (value => ushort.Parse(value, N), value => ((ushort)value).ToString(N)),
        [typeof(uint)] = (value => uint.Parse(value, N), value => ((uint)value).ToString(N)),
        [typeof(ulong)] = (value => ulong.Parse(value, N), value => ((ulong)value).ToString(N)),
        [typeof(DateOnly)] = (value => DateOnly.Parse(value, N), value => ((DateOnly)value).ToString("s")),
        [typeof(TimeOnly)] = (value => TimeOnly.Parse(value, N), value => ((TimeOnly)value).ToString("s")),
        [typeof(Uri)] = (value => new Uri(value), value => ((Uri)value).ToString()),
        [typeof(FileInfo)] = (value => new FileInfo(value), value => ((FileInfo)value).ToString()),
        [typeof(DirectoryInfo)] = (value => new DirectoryInfo(value), value => ((DirectoryInfo)value).ToString()),
        [typeof(Guid)] = (value => new Guid(value), value => ((Guid)value).ToString()),
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