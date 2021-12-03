namespace Woof.Internals.CommandLine;

/// <summary>
/// Provides properties and methods to parse command line arguments.
/// </summary>
class ArgumentParser {

    /// <summary>
    /// Gets the type of the command line argument.
    /// </summary>
    public ArgumentType Type { get; }

    /// <summary>
    /// Gets the parsed value of the argument.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a parsed argument from a command line argument.
    /// </summary>
    /// <param name="arg">Command line argument.</param>
    /// <param name="syntax">Argument syntax.</param>
    public ArgumentParser(string arg, ArgumentSyntax syntax) {
        var d = new OptionSyntaxDefinition(syntax);
        if (d.LongPrefix is not null && arg.StartsWith(d.LongPrefix)) {
            Type = ArgumentType.Key;
            Value = arg[d.LongPrefix.Length..];
        }
        else if (d.ShortPrefix is not null && arg.StartsWith(d.ShortPrefix)) {
            Type = d.LongPrefix is null ? ArgumentType.Key : ArgumentType.Options;
            Value = arg[d.ShortPrefix.Length..];
        }
        else {
            Type = ArgumentType.Value;
            Value = arg;
        }
    }

    /// <summary>
    /// Matches a setter expression.
    /// </summary>
    /// <param name="candidate">Option without the prefix.</param>
    /// <param name="separators">Setter separator characters.</param>
    /// <param name="key">Matched key.</param>
    /// <param name="value">Matched value.</param>
    /// <returns>True if the candidate is a setter.</returns>
    public static bool MatchSetter(string candidate, string separators, out string? key, out string? value) {
        Regex expression = new($"^([0-9A-Za-z_,.?!#$%^&*()]+)[{separators}](.+)$", RegexOptions.Compiled);
        var match = expression.Match(candidate);
        if (match.Success) {
            key = match.Groups[1].Value;
            value = match.Groups[2].Value;
            return true;
        }
        else {
            key = null;
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Converts usage or example format string to another syntax guidelines.<br/>
    /// Works with simple cases, may fail miserably in some specific cases.<br/>
    /// Keep the examples simple enough and it will (probably) work.
    /// </summary>
    /// <param name="format">Format string.</param>
    /// <param name="syntax">Syntax guidelines.</param>
    /// <returns>Converted format string.</returns>
    public static string? ConvertSyntax(string? format, ArgumentSyntax syntax) {
        if (format is null) return null;
        if (format.Length < 2) return format;
        var d = new OptionSyntaxDefinition(syntax);
        IEnumerable<string> split() { // splits the format into non prefixed and prefixed interleaved parts.
            char p = ' '; // previous character
            bool m = false; // match text state
            int ns = 0; // non prefixed region start
            int ps = 0; // prefixed region start
            int end = format.Length; // end of format
            for (int i = 0, n = format.Length; i < n; i++) { // finite state machine parsing
                var c = format[i];
                var isStart = !m && (p == ' ' || p == '|' || p == '(' || p == '<' || p == '[' || p == '{') && (c == '-' || c == '/');
                var isEnd = m && (i == n - 1 || c == ' ' || c == '|' || c == ')' || c == '>' || c == ']' || c == '}');
                if (isStart) { m = true; ps = i; yield return format[ns..ps]; }
                if (isEnd) { m = false; yield return format[ps..i]; ns = i; }
                p = c;
            }
            if (ns < end) yield return format[ns..end];
        }
        var parts = split();
        var converted = parts.Select(part => {
            if (part.Length > 0 && (part[0] == '-' || part[0] == '/')) { // is option
                var prefixLength = part.Length > 1 && part[1] == '-' ? 2 : 1;
                var unprefixed = part[prefixLength..];
                if (d.CaseConversion is not null) unprefixed = d.CaseConversion(unprefixed);
                return (prefixLength > 1 ? d.LongPrefix ?? d.ShortPrefix : d.ShortPrefix) + unprefixed;
            }
            return part;
        });
        var replacement = string.Join("", converted);
        return replacement;
    }

}
