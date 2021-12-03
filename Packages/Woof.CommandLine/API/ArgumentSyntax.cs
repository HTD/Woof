namespace Woof;

/// <summary>
/// Command line argument syntax.
/// </summary>
public enum ArgumentSyntax {

    /// <summary>
    /// Argument doesn't follow any syntax guidelines.
    /// </summary>
    None,

    /// <summary>
    /// Sinlge minus means following characters are options.
    /// Double minus means following characters are full single option name.
    /// Case sensitive.
    /// </summary>
    POSIX,

    /// <summary>
    /// Single minus means following characters are full single option name.
    /// Case insensitive.
    /// </summary>
    PowerShell,

    /// <summary>
    /// Slash followed with a single character means single option. Case insensitive.
    /// </summary>
    DOS

}
