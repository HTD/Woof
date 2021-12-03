namespace Woof.Internals.CommandLine;

/// <summary>
/// Specifies properties of command line option metadata.
/// </summary>
interface IOptionMetadata {

    /// <summary>
    /// Gets or sets the option aliases separated with pipe symbol.
    /// </summary>
    public string Aliases { get; set; }

    /// <summary>
    /// Gets or sets the optional value placeholder name.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional option description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets how the option is required in the command line.
    /// </summary>
    public OptionRequired Required { get; set; }

}
