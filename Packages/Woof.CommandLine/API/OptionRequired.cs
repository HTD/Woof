namespace Woof;

/// <summary>
/// Specifies how the command line option is required in the command line.
/// </summary>
public enum OptionRequired {

    /// <summary>
    /// The option is not required in the command line.
    /// </summary>
    No,
    /// <summary>
    /// Exactly one option with this value is allowed.
    /// </summary>
    One,
    /// <summary>
    /// Any option with this value must occur in the command line.
    /// </summary>
    Any,
    /// <summary>
    /// All options marked with this value must occur in the command line.
    /// </summary>
    All

}
