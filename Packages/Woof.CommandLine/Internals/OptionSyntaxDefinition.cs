namespace Woof.Internals.CommandLine;

class OptionSyntaxDefinition {

    public string? ShortPrefix { get; set; }

    public string? LongPrefix { get; set; }

    public StringComparer StringComparer { get; set; }

    public Func<string, string>? CaseConversion { get; set; }

    /// <summary>
    /// Creates the syntax definition for the specified command line arguments syntax guidelines.
    /// </summary>
    /// <param name="syntax">Command line arguments syntax guidelines.</param>
    public OptionSyntaxDefinition(ArgumentSyntax syntax) {
        switch (syntax) {
            case ArgumentSyntax.POSIX:
                ShortPrefix = "-";
                LongPrefix = "--";
                StringComparer = StringComparer.Ordinal;
                break;
            case ArgumentSyntax.PowerShell:
                ShortPrefix = "-";
                StringComparer = StringComparer.OrdinalIgnoreCase;
                CaseConversion = s => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);
                break;
            case ArgumentSyntax.DOS:
                ShortPrefix = "/";
                StringComparer = StringComparer.OrdinalIgnoreCase;
                CaseConversion = s => s.ToUpper();
                break;
            default:
                StringComparer = StringComparer.Ordinal;
                break;
        }
    }

}
