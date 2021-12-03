namespace Woof;

/// <summary>
/// Command line option metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class OptionAttribute : Attribute, IOptionMetadata {

    /// <summary>
    /// Gets or sets the equivalent command option names separated with the pipe symbol.
    /// </summary>
    public string Aliases { get; set; }

    /// <summary>
    /// Gets or sets the optional value placeholder name. Set if the option accepts a value. Use snake case descriptive name.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the description for automated documentation. Can be overriden with <see cref="CommandLineParser.LocalizationProvider"/>.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets how the option is required in the command line.
    /// </summary>
    public OptionRequired Required { get; set; }

    /// <summary>
    /// Gets or sets a limit to a specific OS platform.
    /// Must be either null or set to <see cref="System.Runtime.InteropServices.OSPlatform"/> member name.
    /// </summary>
    public string? OSPlatform { get; set; }

    /// <summary>
    /// Creates new <see cref="OptionAttribute"/>.
    /// </summary>
    /// <param name="aliases">Equivalent command option names separated with the pipe symbol.</param>
    /// <param name="value">Optional value placeholder name. Set if the option accepts a value. Use snake case descriptive name.</param>
    /// <param name="description">Description for automated documentation. Can be overriden with <see cref="CommandLineParser.LocalizationProvider"/>.</param>
    public OptionAttribute(string aliases, string? value = null, string? description = null) {
        Aliases = aliases;
        Value = value;
        Description = description;
    }

}
