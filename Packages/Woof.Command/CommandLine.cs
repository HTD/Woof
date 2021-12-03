namespace Woof.Command;

/// <summary>
/// Optionally interactive command line processor / console renderer.
/// </summary>
public class CommandLine : ICommand {

    #region Properties

    /// <summary>
    /// Gets the command arguments processed.
    /// </summary>
    public CommandLineArguments Arguments { get; private set; } = new();

    /// <summary>
    /// Gets the exact command alone, without arguments.
    /// </summary>
    public string Command { get; private set; } = String.Empty;

    /// <summary>
    /// Gets the current command line length.
    /// </summary>
    public int Length => _Text.Length;

    /// <summary>
    /// Gets or sets command line text.
    /// </summary>
    public string Text {
        get => _Text;
        set {
            _Text = value;
            Parse();
        }
    }

    /// <summary>
    /// Parts map.
    /// </summary>
    internal int[] Map { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new empty command line.
    /// </summary>
    public CommandLine() => Map = Array.Empty<int>();

    /// <summary>
    /// Creates a new command line from string.
    /// </summary>
    /// <param name="line">Source command line.</param>
    public CommandLine(string line) {
        Map = new int[line.Length];
        Text = line;
    }

    #endregion

    #region Static methods

    /// <summary>
    /// Joins the parts into string separated with spaces.
    /// </summary>
    /// <param name="parts">Parts.</param>
    /// <returns>Line.</returns>
    public static string Join(string[] parts) => String.Join(" ", parts.Select(i => Quote(i)));

    /// <summary>
    /// Quotes the part if the part contains one or more spaces and isn't already quoted.
    /// </summary>
    /// <param name="part">A single command line part.</param>
    /// <returns>Quoted part.</returns>
    public static string Quote(string part) => part.Contains(' ') && part[0] != '"' && part[^1] != '"' ? ('"' + part.Replace("\"", "\"\"") + '"') : part;

    /// <summary>
    /// Unquotes the part if its quoted with double quotes. Also unquotes incomplete quoting.
    /// </summary>
    /// <param name="part">A single command line part.</param>
    /// <returns>Unquoted part.</returns>
    public static string Unquote(string part) => _RxUnquotedDoubleQuotes.Replace(part, "");

    /// <summary>
    /// Splits the command line with space character, shell style.
    /// Quotes (both single and double) prevent space from being a separator.
    /// </summary>
    /// <param name="line">A line to split.</param>
    /// <param name="leaveQuotes">If set true, quoted parts will still contain quotes.</param>
    /// <returns>Parts.</returns>
    public static string[] Split(string line, bool leaveQuotes = false) {
        var result = new List<string>();
        var builder = new StringBuilder();
        var q = '-';
        for (int i = 0, n = line.Length; i < n; i++) {
            char c = line[i];
            if (i > 0 && line[i - 1] == c && (c == '\'' || c == '"')) builder.Append(c);
            if (q == '-' && (c == '\'' || c == '"')) { q = c; if (leaveQuotes) builder.Append(c); continue; }
            if (c != '-' && c == q) { q = '-'; if (leaveQuotes) builder.Append(c); continue; }
            if (q == '-' && c == ' ') {
                if (builder.Length > 0) result.Add(builder.ToString());
                builder.Clear();
            }
            else builder.Append(c);
        }
        if (builder.Length > 0) result.Add(builder.ToString());
        return result.ToArray();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Resets properties. The optional text is parsed.
    /// </summary>
    private void Reset() {
        _Text = String.Empty;
        Command = String.Empty;
        Arguments = new();
        Map = Array.Empty<int>();
    }

    /// <summary>
    /// Gets the part index at specified position within the command line text.
    /// </summary>
    /// <param name="at">Position within the command line text.</param>
    /// <returns>Part index, negative for whitespace that doesn't belong to any part.</returns>
    private int GetPartIndex(int at) => at < 0 ? 0 : Map[at < Text.Length ? at : Text.Length - 1];

    /// <summary>
    /// Splits the command line into command and arguments, creates parts map.
    /// </summary>
    private void Parse() {
        if (String.IsNullOrEmpty(Text)) { Reset(); return; }
        if (Map.Length != Text.Length) Map = new int[Text.Length];
        var unquoted = Split(Text);
        var quoted = Split(Text, true);
        int partIndex = -1, partLeft = -1;
        bool isOutside = true;
        for (int i = 0, n = Map.Length; i < n; i++) {
            if (isOutside) {
                if (Text[i] == ' ') Map[i] = -1;
                else {
                    isOutside = false;
                    partIndex++;
                    Map[i] = partIndex;
                    partLeft = quoted[partIndex].Length;
                }
                continue;
            }
            else {
                Map[i] = partIndex;
                if (--partLeft < 1) isOutside = true;
            }
        }
        Command = unquoted.FirstOrDefault() ?? String.Empty;
        Arguments = new CommandLineArguments(unquoted.Length > 0 ? unquoted[1..] : Array.Empty<string>());
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Implicit <see cref="string"/> conversion, just returns the <see cref="CommandLine.Text"/>.
    /// </summary>
    /// <param name="l"></param>
    public static implicit operator string(CommandLine l) => l.Text;

    /// <summary>
    /// Implicit <see cref="CommandLine"/> conversion, creates new <see cref="CommandLine"/> from <see cref="string"/>.
    /// </summary>
    /// <param name="s"></param>
    public static implicit operator CommandLine(string s) => new(s);

    #endregion

    #region Private data

    /// <summary>
    /// Command line text cache.
    /// </summary>
    private string _Text = String.Empty;

    /// <summary>
    /// Matches douoble quotes.
    /// </summary>
    private static readonly Regex _RxUnquotedDoubleQuotes = new(@"""{1}", RegexOptions.Compiled);

    #endregion

}
