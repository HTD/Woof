namespace Woof;

/// <summary>
/// Command line usage description.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class UsageAttribute : Attribute {

    /// <summary>
    /// Gets or sets the definition format.<br/>
    /// The {command} string is resolved to the current executable.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Gets or sets the example of using the command.<br/>
    /// The {command} string is resolved to the current executable.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Describes the command usage.
    /// </summary>
    /// <param name="format">
    /// Usage command format definition.<br/>
    /// The {command} string is resolved to the current executable.
    /// </param>
    public UsageAttribute(string format) => Format = format;

}
