namespace Woof.ConsoleTools;

/// <summary>
/// Header items to display in assembly header text.
/// </summary>
[Flags]
public enum HeaderItems {

    /// <summary>
    /// Nothing.
    /// </summary>
    None = 0,

    /// <summary>
    /// Assembly title.
    /// </summary>
    Title = 1,

    /// <summary>
    /// Assembly version.
    /// </summary>
    Version = 2,

    /// <summary>
    /// Copyright text.
    /// </summary>
    Copyright = 4,

    /// <summary>
    /// Description text.
    /// </summary>
    Description = 8,

    /// <summary>
    /// Basic information.
    /// </summary>
    Basic = Title + Version,

    /// <summary>
    /// Extended information.
    /// </summary>
    Extended = Title + Version + Copyright,

    /// <summary>
    /// Full header.
    /// </summary>
    All = Title + Version + Copyright + Description

}
