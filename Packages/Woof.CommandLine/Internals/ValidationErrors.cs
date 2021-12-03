namespace Woof.Internals.CommandLine;

/// <summary>
/// Default validation error messages.
/// </summary>
public static class ValidationErrors {

    /// <summary>
    /// Exactly one of these options must be set, none is.
    /// </summary>
    public const string OneMissing = "Exactly one of these options must be set, none is";

    /// <summary>
    /// Exactly one of these options must be set, more than one is.
    /// </summary>
    public const string OneExceeded = "Exactly one of these options must be set, more than one is";

    /// <summary>
    /// At least one of these options must be set, none is.
    /// </summary>
    public const string AnyMissing = "At least one of these options must be set, none is";

    /// <summary>
    /// All of these options must be set, at least one isn't.
    /// </summary>
    public const string AllMissing = "All of these options must be set, at least one isn't";

    /// <summary>
    /// Invalid option.
    /// </summary>
    public const string InvalidOption = "Invalid option";

    /// <summary>
    /// Not enough parameters.
    /// </summary>
    public const string NotEnoughParameters = "Minimal number of parameters: {0}, provided: {1}";

    /// <summary>
    /// Too many parameters.
    /// </summary>
    public const string TooManyParameters = "Maximal number of parameters: {0}, provided: {1}";

    /// <summary>
    /// Gets the default validation error text by name.
    /// </summary>
    /// <param name="name">Error name. One of the constants defined in <see cref="ValidationErrors"/>.</param>
    /// <returns>Default validation error text or just the name if the constant doesn't exist in <see cref="ValidationErrors"/>.</returns>
    public static string GetByName(string name) => (string?)typeof(ValidationErrors).GetField(name)?.GetRawConstantValue() ?? name;

    /// <summary>
    /// Formats validation errors as one block of text.
    /// </summary>
    /// <param name="errors">Validation errors.</param>
    /// <param name="localizationProvider">Localization provider.</param>
    /// <returns>Error text.</returns>
    public static string? ToString(IEnumerable<string> errors, Func<string, string?>? localizationProvider = null) {
        if (!errors.Any()) return null;
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Format.LocalizedString(localizationProvider, "Error", "ERROR"));
        stringBuilder.AppendLine(":");
        foreach (var error in errors) stringBuilder.AppendLine(error);
        return stringBuilder.ToString().TrimEnd();
    }

}
