using System.Collections;

namespace Woof.Command;

/// <summary>
/// AutoComplete list module for <see cref="CommandShell"/>.
/// </summary>
public class CommandAutoCompleteList : IEnumerable<string> {

    #region Properties

    /// <summary>
    /// Gets the number of matching completions.
    /// </summary>
    public int Count => Matching.Length;

    /// <summary>
    /// Gets or sets current completion index.
    /// </summary>
    public int CurrentIndex { get; private set; }

    /// <summary>
    /// Gets the matching file system path or null if matching within current path.
    /// </summary>
    public string? MatchingPath { get; private set; }

    /// <summary>
    /// Gets or sets default color used to show auto complete peek items.
    /// </summary>
    public ConsoleColor PeekColor { get; set; } = ConsoleColor.DarkGreen;

    /// <summary>
    /// Gets or sets the maximum number of items shown as auto complete peek.
    /// </summary>
    public int PeekMax { get; set; } = 255;

    /// <summary>
    /// Gets the mathing completion by specified index.
    /// </summary>
    /// <param name="index">The index of the element on the list.</param>
    /// <returns></returns>
    public string? this[int index] => index >= 0 && index < Matching.Length ? Matching[index] : null;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new <see cref="CommandAutoCompleteList"/> initialized from a collection of commands.
    /// </summary>
    /// <param name="commands">A collection of commands to initialize the list.</param>
    public CommandAutoCompleteList(IEnumerable<string>? commands = null) {
        Commands = commands?.ToArray() ?? [];
        Reload();
        CurrentIndex = -1;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Extends the commands list.
    /// </summary>
    /// <param name="commands">Any string collection.</param>
    public void AddCommands(IEnumerable<string> commands) => Commands = [.. Commands, .. commands.Where(i => !Commands.Contains(i, StringComparer.OrdinalIgnoreCase))];

    /// <summary>
    /// Filters the list by the matching start string.
    /// </summary>
    /// <param name="start">Common start for the completions.</param>
    /// <param name="includeCommands">Set true to include internal commands.</param>
    /// <returns>Offset where the completion match or -1 when nothing matches.</returns>
    public int Match(string? start = null, bool includeCommands = false) {
        MatchingPath = null;
        if (String.IsNullOrWhiteSpace(start)) {
            Reload(".", includeCommands);
            Matching = All;
            return 0;
        }
        start = CommandLine.Unquote(start);
        var lastSlashIndex = start.LastIndexOf(Path.DirectorySeparatorChar);
        if (lastSlashIndex >= 0) {
            var path = start[..(lastSlashIndex + 1)];
            start = start[(lastSlashIndex + 1)..];
            Reload(MatchingPath = path);
            Matching = String.IsNullOrEmpty(start) ? All : All.Where(i => i.StartsWith(start, StringComparison.OrdinalIgnoreCase)).ToArray();
            return lastSlashIndex + 1;
        }
        Reload(".", includeCommands);
        Matching = All.Where(i => i.StartsWith(start, StringComparison.OrdinalIgnoreCase)).ToArray();
        return Matching.Any() ? 0 : -1;
    }

    /// <summary>
    /// Gets the next matching auto complete item.
    /// </summary>
    /// <returns>Matching item or null.</returns>
    public string? Next() {
        if (!Matching.Any()) return null;
        if (CurrentIndex >= Matching.Length || CurrentIndex < 0) CurrentIndex = 0;
        return MatchingPath == null ? Matching[CurrentIndex++] : Path.Combine(MatchingPath, Matching[CurrentIndex++]);
    }

    /// <summary>
    /// Reloads file system entries for the directory, adds commands to the list.
    /// </summary>
    /// <param name="dir">Directory to feed the items list.</param>
    /// <param name="includeCommands">Set true to include internal commands.</param>
    private void Reload(string dir = ".", bool includeCommands = false) {
        if (dir != ".") includeCommands = false;
        static string trim(string _path, string _dir)
            => _path.StartsWith(_dir, StringComparison.OrdinalIgnoreCase) ? _path[_dir.Length..].Trim('\\') : _path;
        if (Directory.Exists(dir)) {
            FileSystemEntries = Directory.EnumerateFileSystemEntries(dir, "*").Select(i => trim(i, dir)).ToArray();
            All = includeCommands ? [.. Commands.Concat(FileSystemEntries).OrderBy(i => i)] : [.. FileSystemEntries.OrderBy(i => i)];
        }
        else {
            FileSystemEntries = [];
            All = includeCommands ? [.. Commands.OrderBy(i => i)] : FileSystemEntries;
        }
    }

    /// <summary>
    /// Resets the auto complete index and clears peek preview if shown.
    /// </summary>
    public void Reset() {
        if (Peek1 >= 0 && Peek2 >= 0) Clear();
        Matching = [];
        CurrentIndex = Peek1 = Peek2 = OriginX = OriginY - 1;
    }

    /// <summary>
    /// Shows the peek preview of available completions.
    /// </summary>
    public void Peek() {
        if (!Matching.Any()) return;
        OriginX = Console.CursorLeft;
        OriginY = Console.CursorTop;
        var peek = Matching.Length < PeekMax ? Matching : Matching.Take(PeekMax).Concat(new[] { "..." });
        var itemWidth = peek.Select(i => i.Length).Max() + 2;
        var windowWidth = Console.WindowWidth;
        while (windowWidth % itemWidth > 0) itemWidth++;
        Console.CursorVisible = false;
        Console.CursorLeft = 0;
        Console.CursorTop++;
        Peek1 = Console.CursorTop;
        var foregroundCurrent = Console.ForegroundColor;
        Console.ForegroundColor = PeekColor;
        foreach (var item in peek) Console.Write(item.PadRight(itemWidth));
        Console.ForegroundColor = foregroundCurrent;
        Peek2 = Console.CursorTop;
        Console.SetCursorPosition(OriginX, OriginY);
        Console.CursorVisible = true;
    }

    /// <summary>
    /// Clears the peek preview.
    /// </summary>
    private void Clear() {
        if (Peek1 < 0 || Peek2 < 0) return;
        Console.CursorVisible = false;
        Console.SetCursorPosition(0, Peek1);
        Console.Write("".PadRight((Peek2 - Peek1 + 1) * Console.WindowWidth));
        Console.SetCursorPosition(OriginX, OriginY);
        Console.CursorVisible = true;
        CurrentIndex = OriginX = OriginY = -1;
    }

    #endregion

    #region IEnumerable implementation

    /// <summary>
    /// Enumerats string items.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Matching).GetEnumerator();

    /// <summary>
    /// Enumerates items.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)Matching).GetEnumerator();

    #endregion

    #region Private data

    private string[] Commands = [];
    private string[] FileSystemEntries = [];
    private string[] All = [];
    private string[] Matching = [];
    private int OriginX;
    private int OriginY;

    private int Peek1 = -1;
    private int Peek2 = -1;

    #endregion

}
