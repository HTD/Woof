namespace Woof;

/// <summary>
/// Advanced command line arguments parser.
/// </summary>
public class CommandLineParser {

    #region API

    #region Configuration

    /// <summary>
    /// Gets or sets the syntax guidelines using to parse the command line arguments.
    /// Default: <see cref="ArgumentSyntax.POSIX"/>.
    /// </summary>
    public ArgumentSyntax Syntax { get; set; } = ArgumentSyntax.POSIX;

    /// <summary>
    /// Gets or sets 1 argument setter separators.
    /// </summary>
    public string SetterSeparators { get; set; } = ":=";

    /// <summary>
    /// Gets or sets the minimal number of accepted parameters not related to an option. Default 0.
    /// </summary>
    public int ParametersMin { get; set; }

    /// <summary>
    /// Gets or sets the maximal number of accepted parameters not related to an option. Null for no limit.
    /// </summary>
    public int? ParametersMax { get; set; }

    /// <summary>
    /// Gets or set a value indicating whether the <see cref="OptionsUndefined"/> should be ignored on validation.
    /// </summary>
    public bool IgnoreOptionsUndefined { get; set; }

    /// <summary>
    /// Gets or sets the function used to resolve localized text for identifiers.
    /// </summary>
    public Func<string, string>? LocalizationProvider { get; set; }

    /// <summary>
    /// Gets the option to delegate bindings.
    /// A delegate should be synchronous or asynchronous method optionally accepting a string.
    /// </summary>
    public Dictionary<Enum, Delegate> Delegates { get; } = new();

    #endregion

    #region Parsing results

    /// <summary>
    /// Gets the command agruments passed to the parse method.
    /// </summary>
    public string[]? Args { get; private set; }

    /// <summary>
    /// Gets the command line data (options with values).
    /// </summary>
    public Dictionary<Enum, string?> Data { get; } = new();

    /// <summary>
    /// Gets the command line option identifiers.
    /// </summary>
    public HashSet<Enum> Options { get; } = new();

    /// <summary>
    /// Gets the undefined options in the parsed command line.
    /// </summary>
    public HashSet<string> OptionsUndefined { get; } = new();

    /// <summary>
    /// Gets the command line parameters: arguments not being option keys or values.
    /// </summary>
    public List<string> Parameters { get; } = new();

    /// <summary>
    /// Gets the detailed validation error descriptions if any.
    /// </summary>
    public string[] ValidationErrors => CurrentMap.Validate(
            Options,
            IgnoreOptionsUndefined ? null : OptionsUndefined,
            Parameters.Count,
            ParametersMin,
            ParametersMax,
            Syntax,
            LocalizationProvider
        ).ToArray();

    #endregion

    /// <summary>
    /// Clears command line parsing data, use to parse subsequent command line argument arrays.
    /// </summary>
    public void Reset() {
        Data.Clear();
        Options.Clear();
        OptionsUndefined.Clear();
        Parameters.Clear();
        Delegates.Clear();
    }

    /// <summary>
    /// Adds a configured options enumeration to the command line mapping.
    /// Affects how the arguments are parsed.
    /// </summary>
    /// <typeparam name="TEnum">Options enumeration type.</typeparam>
    /// <param name="withReplace">Replace the current map.</param>
    public void Map<TEnum>(bool withReplace = false) where TEnum : struct, Enum {
        if (withReplace) CurrentMap.Clear();
        var usageString = GetUsageString<TEnum>();
        if (usageString is not null) CommandLine.Usage = usageString;
        foreach (var item in OptionMetadata.Get<TEnum>())
            if (!CurrentMap.Any(e => e.Option.Equals(item.Option))) CurrentMap.Add(item);
    }

    /// <summary>
    /// Parses the command line arguments against previously mapped options.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="withReset">Set true if previously matched options should be reset.</param>
    public void Parse(string[] args, bool withReset = false) {
        Args = args;
        if (withReset) Reset();
        else Parameters.Clear();
        string? key = null;
        string? value = null;
        OptionMetadata? match = null;
        OptionMetadata? lastMatch = null;
        foreach (var parsed in args.Select(arg => new ArgumentParser(arg, Syntax))) {
            switch (parsed.Type) {
                case ArgumentType.Key:
                    if (ArgumentParser.MatchSetter(parsed.Value, SetterSeparators, out key, out value)) {
                        match = MatchToAlias(key!);
                        if (match is not null && match.Value is not null) {
                            Options.Add(match);
                            Data[match] = value;
                        }
                        else OptionsUndefined.Add(key!);
                    }
                    else {
                        match = MatchToAlias(parsed.Value);
                        if (match is not null) {
                            Options.Add(match);
                            if (match.Value is not null) Data[match] = null;
                            lastMatch = match;
                        }
                        else OptionsUndefined.Add(parsed.Value);
                    }
                    break;
                case ArgumentType.Value:
                    if (lastMatch is not null && lastMatch.Value is not null) Data[lastMatch] = parsed.Value;
                    else Parameters.Add(parsed.Value);
                    lastMatch = null;
                    break;
                case ArgumentType.Options:
                    foreach (var c in ArgumentParser.MatchSetter(parsed.Value, SetterSeparators, out key, out value) ? key! : parsed.Value) {
                        match = MatchToAlias(c.ToString());
                        if (match is not null) {
                            Options.Add(match);
                            if (match.Value is not null) Data[match] = null;
                            lastMatch = match;
                        }
                        else OptionsUndefined.Add(c.ToString());
                    }
                    if (lastMatch is not null && value is not null) {
                        Data[lastMatch] = value;
                        lastMatch = null;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Runs delegates matched in the command line synchronously.
    /// </summary>
    public void RunDelegates() {
        foreach (var option in Options) {
            if (!Delegates.ContainsKey(option)) continue;
            var @delegate = Delegates[option];
            var isExpectingValue = CurrentMap.Any(m => m.Option.Equals(option) && m.Value is not null);
            var value = isExpectingValue ? Value(option) : null;
            if (isExpectingValue) @delegate.CallAsync(value).Wait();
            else @delegate.CallAsync().Wait();
        }
    }

    /// <summary>
    /// Runs delegates matched in the command line asynchronously.
    /// </summary>
    public async Task RunDelegatesAsync() {
        foreach (var option in Options) {
            if (!Delegates.ContainsKey(option)) continue;
            var @delegate = Delegates[option];
            var isExpectingValue = CurrentMap.Any(m => m.Option.Equals(option) && m.Value is not null);
            var value = isExpectingValue ? Value(option) : null;
            if (isExpectingValue) await @delegate.CallAsync(value);
            else await @delegate.CallAsync();
        }
    }

    /// <summary>
    /// Checks if the command line contains an option.
    /// </summary>
    /// <param name="option">Options enumeration value.</param>
    /// <returns>True if option exists in the command line arguments parsed.</returns>
    public bool HasOption(Enum option) => Options.Contains(option);

    /// <summary>
    /// Gets the specified option value if it exists in the command line arguments parsed.
    /// </summary>
    /// <param name="option">Options enumeration value.</param>
    /// <returns>Option value if exists, null otherwise.</returns>
    public string? Value(Enum option) => Data.GetValueOrDefault(option);

    #endregion

    #region Implementation details

    /// <summary>
    /// Matches metadata from a map by alias.
    /// </summary>
    /// <param name="alias">String matched to metadata alias.</param>
    /// <returns>Option metadata or null if not matched.</returns>
    private OptionMetadata? MatchToAlias(string alias)
        => CurrentMap.FirstOrDefault(m => m.Aliases is not null && m.Aliases.Split('|').Contains(alias, StringComparer));

    /// <summary>
    /// Gets the usage if special attribute is defined in the options enumeration.
    /// </summary>
    /// <typeparam name="TEnum">Options enumeration.</typeparam>
    /// <returns>Processed usage text if available, null otherwise.</returns>
    private string? GetUsageString<TEnum>() where TEnum : struct, Enum {
        var type = typeof(TEnum);
        var usageAttribute = type.GetCustomAttribute<UsageAttribute>();
        if (usageAttribute == null) return null;
        var usageLine = usageAttribute.Format;
        var exampleLine = usageAttribute.Example;
        var fileName = new FileInfo(System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName!).Name;
        var command = fileName.EndsWith(".dll")
            ? $"dotnet {fileName}"
            : fileName.EndsWith(".exe")
                ? fileName[0..^4]
                : fileName;
        usageLine = usageLine.Replace("{command}", command);
        usageLine = ArgumentParser.ConvertSyntax(usageLine, Syntax);
        if (exampleLine is not null) {
            exampleLine = exampleLine.Replace("{command}", command);
            exampleLine = ArgumentParser.ConvertSyntax(exampleLine, Syntax);
        }
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Format.LocalizedString(LocalizationProvider, "Usage", "USAGE"));
        stringBuilder.AppendLine();
        stringBuilder.Append("  ");
        stringBuilder.AppendLine(usageLine);
        if (exampleLine is not null) {
            stringBuilder.Append(Format.LocalizedString(LocalizationProvider, "Example", "EXAMPLE"));
            stringBuilder.AppendLine();
            stringBuilder.Append("  ");
            stringBuilder.AppendLine(exampleLine);
        }
        return stringBuilder.ToString();
    }

    #region Private data

    /// <summary>
    /// Maps containing command line options binding metadata.
    /// </summary>
    internal readonly List<OptionMetadata> CurrentMap = new();

    /// <summary>
    /// Gets the <see cref="StringComparer"/> depending on the <see cref="Syntax"/>.
    /// </summary>
    private StringComparer StringComparer
        => Syntax == ArgumentSyntax.POSIX ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

    #endregion

    #endregion

}
