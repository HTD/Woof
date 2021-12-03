namespace Woof.Internals.CommandLine;

/// <summary>
/// Option formatting module.
/// </summary>
static class Format {

    /// <summary>
    /// Gets the specified option metadata as string.
    /// </summary>
    /// <param name="metadata">Option metadata.</param>
    /// <param name="syntax">Argument syntax guidelines.</param>
    /// <param name="separator">Aliases list separator.</param>
    /// <returns>Formatted argument aliases.</returns>
    public static string ToString(this OptionMetadata metadata, ArgumentSyntax syntax, string separator = ", ") {
        StringBuilder builder = new();
        builder.Append(string.Join(separator, metadata.Aliases.Split('|').Select(e => ToString(e, null, syntax))));
        if (metadata.Value is not null) builder.Append($" <{metadata.Value}>");
        return builder.ToString();
    }

    /// <summary>
    /// Gets the description text for the options.
    /// </summary>
    /// <param name="options">Any <see cref="OptionMetadata"/> collection.</param>
    /// <param name="syntax">Command line argument syntax guideliens.</param>
    /// <param name="localizationProvider">
    /// A function returning localized string using the key, or null if it is not found.
    /// </param>
    /// <param name="indentationLevel">The level of indentation for each line.</param>
    /// <returns>Description text for the options.</returns>
    public static string? ToString(
        this IEnumerable<OptionMetadata> options,
        ArgumentSyntax syntax,
        Func<string, string>? localizationProvider = null,
        int indentationLevel = 2
    ) {
        const int tabWidth = 8;
        IEnumerable<(string, string)> getGrid() {
            string? column1 = null, column2 = null;
            foreach (var metadata in options) {
                if (metadata.Aliases is null) continue;
                column1 = metadata.ToString(syntax);
                if (column1 is null) continue;
                column2 = LocalizedString(localizationProvider, metadata.Option.ToString(), metadata.Description!);
                if (column2 is null) column2 = string.Empty;
                yield return (column1, column2);
            }
        }
        var grid = getGrid().ToArray();
        var column1width = grid.Select(e => e.Item1.Length).Max();
        var tabFloor = column1width / tabWidth * tabWidth;
        column1width = tabFloor + 2 * tabWidth - 2;
        var stringBuilder = new StringBuilder();
        foreach (var row in grid) {
            if (indentationLevel > 0) stringBuilder.Append(string.Empty.PadRight(indentationLevel));
            stringBuilder.Append(row.Item1.PadRight(column1width));
            stringBuilder.AppendLine(row.Item2);
        }
        return stringBuilder.ToString().TrimEnd();
    }

    /// <summary>
    /// Format the option string according to the specified syntax.
    /// </summary>
    /// <param name="name">Option name.</param>
    /// <param name="value">Optional option value placeholder.</param>
    /// <param name="syntax">Syntax guidelines.</param>
    /// <returns>Formatted option string.</returns>
    public static string ToString(string name, string? value, ArgumentSyntax syntax) {
        OptionSyntaxDefinition d = new(syntax);
        var prefix =
            name.Length > 1 && d.LongPrefix is not null
                ? d.LongPrefix
                : d.ShortPrefix;
        if (d.CaseConversion is not null) {
            name = d.CaseConversion(name);
            if (value is not null) value = d.CaseConversion(value);
        }
        return value is null
            ? $"{prefix}{name}"
            : $"{prefix}{name} <{value}>";
    }

    /// <summary>
    /// Gets the localized string using localization provider method.
    /// </summary>
    /// <param name="localizationProvider">
    /// A function returning localized string using the <paramref name="key"/>, or null if it is not found.
    /// </param>
    /// <param name="key">A key used to find the localized string.</param>
    /// <param name="defaultValue">A value to return, when either <paramref name="localizationProvider"/> is null, or the localized string is not found.</param>
    /// <returns>Localized string or <paramref name="defaultValue"/>.</returns>
    public static string LocalizedString(Func<string, string?>? localizationProvider, string key, string defaultValue)
        => localizationProvider is null ? defaultValue : localizationProvider(key) ?? defaultValue;

}
