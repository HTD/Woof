namespace Woof.Settings;

/// <summary>
/// Defines from string and to string conversions for the system types.
/// </summary>
/// <remarks>
/// Implemented as a dictionary of type keys and tuples of values:<br/>
/// <see cref="ValueParser"/> Parse delegate,<br/>
/// <see cref="ValueToString"/> GetString delegate,<br/>
/// <see cref="bool"/> IsQuoted.
/// </remarks>
public class ValueConversions : Dictionary<Type, (ValueParser Parse, ValueToString GetString, bool IsQuoted)> {

    /// <summary>
    /// Gets the default value conversions for the system types.
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
        [typeof(IPAddress)] = (value => IPAddress.Parse(value), value => ((IPAddress)value).ToString(), true),
        [typeof(IPEndPoint)] = (value => IPEndPoint.Parse(value), value => ((IPEndPoint)value).ToString(), true),
        [typeof(FileInfo)] = (value => new FileInfo(value), value => ((FileInfo)value).ToString(), true),
        [typeof(DirectoryInfo)] = (value => new DirectoryInfo(value), value => ((DirectoryInfo)value).ToString(), true),
        [typeof(Guid)] = (value => new Guid(value), value => ((Guid)value).ToString(), true)
    };

    /// <summary>
    /// Tests if the type serialization is supported.
    /// </summary>
    /// <param name="type">Type to test.</param>
    /// <returns>True if it is supported.</returns>
    public static bool IsSupported(Type type) => Default.ContainsKey(type) || typeof(ISerializableSettingsValue).IsAssignableFrom(type);

    /// <summary>
    /// Gets the serialized value from object.
    /// </summary>
    /// <param name="value">Value to serialize.</param>
    /// <param name="isQuoted">True if the string representation needs quoting.</param>
    /// <returns>Serialized value.</returns>
    /// <exception cref="InvalidCastException">Unsupported type.</exception>
    public static string GetString(object value, out bool isQuoted) {
        var type = value.GetType();
        if (Default.TryGetValue(type, out (ValueParser Parse, ValueToString GetString, bool IsQuoted) value1)) {
            var (Parse, GetString, IsQuoted) = value1;
            isQuoted = IsQuoted;
            if (value is IProtectedSettingsValue pValue) pValue.Protect();
            return GetString(value);
        }
        if (value is ISerializableSettingsValue sValue) {
            isQuoted = true;
            if (value is IProtectedSettingsValue pValue) pValue.Protect();
            return sValue.GetString();
        }
        throw new InvalidCastException();
    }

    /// <summary>
    /// Gets the deserialized object from string.
    /// </summary>
    /// <param name="value">Serialized string.</param>
    /// <param name="type">Target type.</param>
    /// <returns>Deserialized object.</returns>
    /// <exception cref="InvalidCastException">Unsupported type.</exception>
    public static object Parse(string value, Type type) {
        if (Default.TryGetValue(type, out var item)) return item.Parse(value);
        if (typeof(ISerializableSettingsValue).IsAssignableFrom(type)) {
            var instance = (ISerializableSettingsValue)Activator.CreateInstance(type)!;
            instance.Parse(value);
            return instance;
        }
        throw new InvalidCastException();
    }

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