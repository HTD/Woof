namespace Woof.Command;

/// <summary>
/// Console message type enumeration.
/// </summary>
public enum CommandMessageType {

    /// <summary>
    /// Generic text content.
    /// </summary>
    Content,

    /// <summary>
    /// Distinguished informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Special distinctive message.
    /// </summary>
    Special,

    /// <summary>
    /// A notice. Nothing wrong here.
    /// </summary>
    Notice,

    /// <summary>
    /// A warning. Probably the user has done something wrong.
    /// </summary>
    Warning,

    /// <summary>
    /// An error. Either the user has entered invalid data, or the command encountered an exception.
    /// </summary>
    Error

}