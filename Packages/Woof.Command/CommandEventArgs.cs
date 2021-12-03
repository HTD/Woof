namespace Woof.Command;

/// <summary>
/// Event arguments for <see cref="Command"/> event.
/// </summary>
public sealed class CommandEventArgs : EventArgs, ICommand {

    /// <summary>
    /// Gets the command without the arguments.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Gets the command arguments processed.
    /// </summary>
    public CommandLineArguments Arguments { get; }

    /// <summary>
    /// Gets the full command line text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Set if the command was handled.
    /// </summary>
    public bool IsHandled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the current shell session should end.
    /// </summary>
    public bool ShouldExit { get; set; }

    /// <summary>
    /// Gets or sets the command output text.
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets the command output text color.
    /// </summary>
    public ConsoleColor? OutputColor { get; set; }

    /// <summary>
    /// Gets or sets the output type, that determines the output color.
    /// </summary>
    public CommandMessageType OutputType { get; set; }

    /// <summary>
    /// Creates new event arguments for the Command event.
    /// </summary>
    /// <param name="commandLine">Command line.</param>
    public CommandEventArgs(CommandLine commandLine) {
        Command = commandLine.Command;
        Arguments = commandLine.Arguments;
        Text = commandLine.Text;
    }

}