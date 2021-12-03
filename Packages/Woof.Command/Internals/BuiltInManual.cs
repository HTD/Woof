namespace Woof.Command.Internals;

/// <summary>
/// Contains the build in manual pages for the <see cref="CommandShell"/>.
/// </summary>
internal class BuiltInManual : Dictionary<string, string> {

    /// <summary>
    /// Creates the manual pages for the built-in commands.
    /// </summary>
    /// <remarks>
    /// Note the Linux line endings added to test the conversion on Windows.
    /// There is no string concatenation at runtime because the "concatenated" strings are constants.
    /// </remarks>
    public BuiltInManual() : base(new Dictionary<string, string> {
        ["cat"] =
            "Usage: cat [FILE]\n" +
            "Concatenates a file to this shell output.",
        ["cd"] =
            "Usage: cd [[DIRECTORY]]\n" +
            "Changes current directory or shows current directory when used without a parameter.",
        ["cls"] =
            "Usage: cls\n" +
            "Clears the console window. Press Ctrl+L instead.",
        ["exit"] =
            "Usage: exit\n" +
            "Exits this shell session. Press Ctrl+D instead.",
        ["history"] =
            "Usage: history [[-c]|[--clear]]\n" +
            "Shows current command history or clear it if '--clear' switch is used.",
        ["ls"] =
            "Usage: ls [[DIRECTORY]]\n" +
            "Lists the detailed content of the current or specified directory.",
        ["man"] =
            "Usage: man [[PAGE]]\n" +
            "Shows a micro-manual for the specified command of this shell.\n" +
            "Shows list of available internal commands when used without [PAGE] parameter.",
        ["pwd"] =
            "Usage: pwd\n" +
            "Shows the path to the current working directory.",
        ["touch"] =
            "Usage: touch [FILE]\n" +
            "Creates a new empty file, or sets the last write time of the existing one to current."

    }) { }

}