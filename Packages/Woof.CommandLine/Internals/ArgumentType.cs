namespace Woof.Internals.CommandLine;

/// <summary>
/// Argument types.
/// </summary>
enum ArgumentType {

    /// <summary>
    /// Key or switch.
    /// </summary>
    Key,

    /// <summary>
    /// Literal value not being a key or switch.
    /// </summary>
    Value,

    /// <summary>
    /// A string containing case sensitive switches.
    /// </summary>
    Options

}
