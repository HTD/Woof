namespace Woof.Shell.Internals;

/// <summary>
/// Space delimited strings parser.
/// </summary>
public class SpaceDelimitedStringParser {

    /// <summary>
    /// Gets or sets the quoting character.
    /// </summary>
    public char Quote { get; set; } = '"';

    /// <summary>
    /// Splits the input by whitespace keeping quoted items.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <returns>Items.</returns>
    public IEnumerable<string> Split(string input) {
        bool isQuoting = false;
        for (int i = 0, s = 0, n = input.Length; i < n; i++) {
            var c = input[i];
            var isWhiteSpace = c is ' ' or '\t';
            var isQuote = c == Quote;
            var isBreak = i == n - 1 || isWhiteSpace && !isQuoting;
            if (isBreak) {
                yield return input[s..i];
                s = i + 1;
                continue;
            }
            if (isWhiteSpace && !isQuoting) continue;
            if (isQuote) {
                if (!isQuoting) {
                    s = i + 1;
                    isQuoting = true;
                }
                else {
                    yield return input[s..i];
                    s = i + 1;
                    isQuoting = false;
                }
            }
        }
    }

    /// <summary>
    /// Joins the items with spaces. Spaces contained in items will be quoted.
    /// </summary>
    /// <param name="items">Items to join.</param>
    /// <returns>Space delimited string.</returns>
    public string Join(IEnumerable<string> items) => String.Join(' ', items.Select(i => AddQuotes(i)));

    /// <summary>
    /// Adds quotes to the item if the item needs it to be placed into space delimited string.
    /// </summary>
    /// <param name="item">Item.</param>
    /// <returns>Optionally quoted item.</returns>
    public string AddQuotes(string item) =>
        item.Length > 1 &&
        item[0] != Quote &&
        item.Contains(' ') &&
        item[^1] != Quote ? (Quote + item + Quote) : item;

    /// <summary>
    /// Strips quotes from the item if present.
    /// </summary>
    /// <param name="item">Item.</param>
    /// <returns>Unquoted item.</returns>
    public string StripQuotes(string item) =>
        item.Length > 1 &&
        item[0] == Quote &&
        item[^1] == Quote ? item[1..^2] : item;

}