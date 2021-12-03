namespace Woof.Command;

/// <summary>
/// Common command line properties.
/// </summary>
public interface ICommand {

    /// <summary>
    /// Gets the command line arguments.
    /// </summary>
    CommandLineArguments Arguments { get; }

    /// <summary>
    /// Gets the command without arguments.
    /// </summary>
    string Command { get; }

    /// <summary>
    /// Gets the full command line text.
    /// </summary>
    string Text { get; }

}