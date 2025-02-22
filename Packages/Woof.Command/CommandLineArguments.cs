using System.Collections;

namespace Woof.Command;

/// <summary>
/// Command line arguments processing class.
/// </summary>
public sealed partial class CommandLineArguments : IEnumerable<string> {

    /// <summary>
    /// Gets a value indicating whether the command line has arguments.
    /// </summary>
    public bool Any { get; }

    /// <summary>
    /// Gets the original raw arguments.
    /// </summary>
    public string[] Raw { get; }

    /// <summary>
    /// Gets all parameters (arguments not prefixed with a switch).
    /// </summary>
    public CommandLinePositionalArgumentCollection Positional { get; }

    /// <summary>
    /// Gets all switches.
    /// </summary>
    public CommandLineSwitchCollection Switches { get; }

    /// <summary>
    /// Gets all options (arguments prefixed with a switch).
    /// </summary>
    public CommandLineOptionCollection Options { get; }

    /// <summary>
    /// Creates empty <see cref="CommandLineArguments"/> instance.
    /// </summary>
    public CommandLineArguments() {
        Any = false;
        Raw = [];
        Positional = new();
        Switches = new();
        Options = new();
    }

    /// <summary>
    /// Parses command line arguments into positional arguments, switches and options.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="markedAsOptions">Optional switches to be treated as options.</param>
    public CommandLineArguments(string[] args, params string[] markedAsOptions) {
        Raw = args;
        Any = Raw.Length > 0;
        var e = GetEnumerator();
        var positional = new List<string>();
        var switches = new List<string>();
        var options = new Dictionary<string, string>();
        while (e.MoveNext()) {
            var match = RxElement.Match(e.Current);
            if (match.Success) {
                var key = match.Groups[1].Value;
                if (markedAsOptions.Any() && markedAsOptions.Contains(key, StringComparer.Ordinal))                     if (e.MoveNext() && !options.ContainsKey(key)) options.Add(key, e.Current);
                else switches.Add(key);
            }
            else positional.Add(e.Current);
        }
        Positional = new CommandLinePositionalArgumentCollection(positional);
        Switches = new CommandLineSwitchCollection(switches);
        Options = new CommandLineOptionCollection(options);
    }

    /// <summary>
    /// Matches element with or without optional switch.
    /// </summary>
    private static readonly Regex RxElement = RxElementCT();

    /// <summary>
    /// Gets the enumerator of the raw original array.
    /// </summary>
    /// <returns>String enumerator.</returns>
    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Raw).GetEnumerator();

    /// <summary>
    /// Gets the enumerator of the raw original array.
    /// </summary>
    /// <returns>Enumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator() => Raw.GetEnumerator();

    [GeneratedRegex("^(?:-|--|/)(.*)$", RegexOptions.Compiled)]
    private static partial Regex RxElementCT();

}
