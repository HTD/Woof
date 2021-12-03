namespace Woof;

/// <summary>
/// Application-wide command line parser.
/// </summary>
public static class CommandLine {

    /// <summary>
    /// Gets the default parser instance for the application.
    /// </summary>
    public static CommandLineParser Default => _Default ??= new CommandLineParser();

    #region Automatic documentation

    /// <summary>
    /// Gets the application command line header built from configured assembly attributes.
    /// </summary>
    public static string? Header {
        get {
            if (_Header is not null) return _Header;
            var assembly = Assembly.GetEntryAssembly();
            if (assembly is null) return null;
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.Modules.FirstOrDefault()?.ScopeName;
            if (title is null) return null;
            var version =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
                assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ??
                assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            var builder = new StringBuilder();
            builder.Append(title);
            if (version is not null) {
                builder.Append(' ');
                builder.Append(version);
            }
            if (copyright is not null) {
                builder.Append(' ');
                builder.Append(copyright);
            }
            builder.AppendLine();
            if (description is not null) builder.AppendLine(description);
            return _Header = builder.ToString();
        }
    }

    /// <summary>
    /// Gets the application usage text if available.
    /// </summary>
    public static string? Usage { get; internal set; }

    /// <summary>
    /// Gets the options usage summary based on the options mapped and <see cref="CommandLineParser.Syntax"/>.
    /// </summary>
    public static string? OptionsSummary {
        get {
            if (Default.CurrentMap is null) return null;
            var optionsText = Default.CurrentMap.ToString(Default.Syntax, Default.LocalizationProvider);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Format.LocalizedString(Default.LocalizationProvider, "Options", "OPTIONS"));
            stringBuilder.AppendLine();
            stringBuilder.Append(optionsText);
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Validates the default command line and returns the validation errors if any.
    /// </summary>
    public static string? ValidationErrors {
        get {
            var messages = Default.ValidationErrors;
            return messages.Length < 1 ? null : Internals.CommandLine.ValidationErrors.ToString(messages, Default.LocalizationProvider);
        }
    }

    /// <summary>
    /// Gets the automatic documentation help containing <see cref="Header"/>, <see cref="Usage"/> and <see cref="OptionsSummary"/>.
    /// </summary>
    public static string? Help {
        get {
            string? header = Header, usage = Usage, options = OptionsSummary;
            if (header is null && usage is null && options is null) return null;
            var builder = new StringBuilder();
            if (header is not null) builder.AppendLine(header);
            if (usage is not null) builder.AppendLine(usage);
            if (options is not null) builder.AppendLine(options);
            return builder.ToString();
        }
    }

    #endregion

    #region Private data

    /// <summary>
    /// <see cref="Default"/> property backing field.
    /// </summary>
    private static CommandLineParser? _Default;

    /// <summary>
    /// <see cref="Header"/> property backing field;
    /// </summary>
    private static string? _Header;

    #endregion

}
