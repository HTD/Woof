namespace Woof.Command;

/// <summary>
/// A dictionary of man pages. The key is the command name, the value is the manual page for the command.
/// </summary>
public class ManPagesDictionary : Dictionary<string, string> {

    /// <summary>
    /// Creates an empty manual pages directory.
    /// </summary>
    public ManPagesDictionary() { }

    /// <summary>
    /// Creates a pre-initialized maual pages dictionary.
    /// </summary>
    /// <param name="dictionary"></param>
    public ManPagesDictionary(Dictionary<string, string> dictionary) : base(dictionary) { }

    /// <summary>
    /// Adds a new command manual page from string.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public new void Add(string key, string value) => base.Add(key, value);

    /// <summary>
    /// Adds a new command manual page from text lines.
    /// </summary>
    /// <param name="key">Command name.</param>
    /// <param name="lines">Lines to add.</param>
    public void Add(string key, IEnumerable<string> lines) => Add(key, String.Join(Environment.NewLine, lines));

}