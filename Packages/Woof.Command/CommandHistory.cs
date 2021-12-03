using System.Collections;

namespace Woof.Command;

/// <summary>
/// Command history lines collection.
/// </summary>
public class CommandHistory : IEnumerable<string> {

    #region Properties

    /// <summary>
    /// Gets the number of stored history lines.
    /// </summary>
    public int Count => Items.Count;

    /// <summary>
    /// Gets or sets current history line index.
    /// </summary>
    public int CurrentIndex { get; set; } = -1;

    /// <summary>
    /// Returns n-th history element counting from the last (for zero).
    /// If there is no history line for the level null is returned.
    /// </summary>
    /// <param name="level">History level. Zero is last, 1 is one before that.</param>
    /// <returns>History line.</returns>
    public string? this[int level] => Peek(level);

    #endregion

    #region Constructors

    /// <summary>
    /// Creates empty <see cref="CommandHistory"/>.
    /// </summary>
    public CommandHistory() { }

    /// <summary>
    /// Creates a command history lines collection from a string collection.
    /// </summary>
    /// <param name="items">A string collection to create the history list from.</param>
    public CommandHistory(IEnumerable<string> items) => Items = items != null ? new List<string>(items) : new List<string>();

    #endregion

    #region Methods

    /// <summary>
    /// Adds the line to the history if there is a line and if it's not the same as the last added one.
    /// </summary>
    /// <param name="line">Command line to add.</param>
    public void Add(string line) {
        if (String.IsNullOrWhiteSpace(line)) return;
        line = line.Trim();
        if (!Items.Any() || !line.Equals(Items[^1], StringComparison.Ordinal)) Items.Add(line);
    }

    /// <summary>
    /// Clears all history lines.
    /// </summary>
    public void Clear() {
        CurrentIndex = -1;
        Items.Clear();
    }

    /// <summary>
    /// Returns n-th history element counting from the last (for zero).
    /// If there is no history line for the level null is returned.
    /// </summary>
    /// <param name="level">History level. Zero is last, 1 is one before that.</param>
    /// <returns>History line.</returns>
    public string? Peek(int level = 0) => !Items.Any() || level < 0 || level > Items.Count - 1 ? null : Items[Items.Count - 1 - level];

    /// <summary>
    /// Returns subsequent history item, each call returns older one.
    /// If current text is available it's added to the history, so the user can get back to that.
    /// </summary>
    /// <param name="current"></param>
    /// <returns>History item.</returns>
    public string? Prev(string? current = null) {
        if (!Items.Any()) return null;
        if (!String.IsNullOrEmpty(current) && CurrentIndex < 0) { Add(current); CurrentIndex++; }
        if (CurrentIndex < -1) CurrentIndex = -1;
        if (CurrentIndex > Items.Count - 2) CurrentIndex = Items.Count - 2;
        return this[++CurrentIndex];
    }

    /// <summary>
    /// Resets history index.
    /// </summary>
    public void Reset() => CurrentIndex = -1;

    /// <summary>
    /// Returns more recetnt history item, each call returns newer one.
    /// If the <see cref="CurrentIndex"/> is negative the call is ignored.
    /// </summary>
    /// <returns>History item.</returns>
    public string? Next() {
        if (!Items.Any() || CurrentIndex < 0) return null;
        if (CurrentIndex < 1) CurrentIndex = 1;
        if (CurrentIndex > Items.Count) CurrentIndex = Items.Count;
        return this[--CurrentIndex];
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Returns the history as subsequent lines.
    /// </summary>
    /// <returns>Subsequent lines of text or null.</returns>
    public override string ToString() => Items.Any() ? String.Join(Environment.NewLine, Items) : String.Empty;

    /// <summary>
    /// Returns the history text except last n lines.
    /// </summary>
    /// <param name="skipLast">Subsequent lines of text or null.</param>
    /// <returns>Subsequent lines of text or null.</returns>
    public string ToString(int skipLast) => Items.Count > skipLast ? String.Join(Environment.NewLine, Items.Take(Items.Count - skipLast)) : String.Empty;

    /// <summary>
    /// Converts command history into plain lines of text.
    /// </summary>
    /// <param name="h"><see cref="CommandHistory"/> instance.</param>
    public static implicit operator string(CommandHistory h) => h.ToString();

    /// <summary>
    /// Converts plain lines of text into command history instance.
    /// </summary>
    /// <param name="s">Plain text lines.</param>
    public static implicit operator CommandHistory(string s) => new(s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

    #endregion

    #region IEnumerable implementation

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Items).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)Items).GetEnumerator();

    #endregion

    #region Private data

    /// <summary>
    /// The internal list of items.
    /// </summary>
    private readonly List<string> Items = new();

    #endregion

}
