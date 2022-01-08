namespace Woof.CSV;

/// <summary>
/// An attribute containing <see cref="DateTime"/> format string.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DateTimeFormatAttribute : Attribute {

    /// <summary>
    /// Gets the format string from the attribute.
    /// </summary>
    public string Format { get; } = "yyyy-MM-dd";

    /// <summary>
    /// Creates new <see cref="DateTimeFormatAttribute"/> from specified format string.
    /// </summary>
    /// <param name="format">Format string.</param>
    public DateTimeFormatAttribute(string format) {
        if (format != null) Format = format;
    }

}

/// <summary>
/// An attribute containing <see cref="TimeSpan"/> format string.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TimeSpanFormatAttribute : Attribute {

    /// <summary>
    /// Gets the format string from the attribute.
    /// </summary>
    public string Format { get; } = "HH:mm";

    /// <summary>
    /// Creates new <see cref="TimeSpanFormatAttribute"/> from specified format string.
    /// </summary>
    /// <param name="format">Format string.</param>
    public TimeSpanFormatAttribute(string format) {
        if (format != null) Format = format;
    }

}
