namespace Woof.Shell;

/// <summary>
/// Exception thrown when command executed with shell fails.
/// </summary>
public class ShellExecException : Exception {

    /// <summary>
    /// Gets the command output if any.
    /// </summary>
    public string? CommandOutput { get; }

    /// <summary>
    /// Gets the exit code of the command.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Creates the new <see cref="ShellExecException"/>.
    /// </summary>
    /// <param name="message">Error message from STDERR.</param>
    /// <param name="exitCode">Exit code.</param>
    /// <param name="output">Output content from STDOUT.</param>
    public ShellExecException(string? message, int exitCode = 0, string? output = null) : base(message ?? output) {
        CommandOutput = output;
        ExitCode = exitCode;
    }

}