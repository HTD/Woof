namespace Woof.Internals.CommandLine;

/// <summary>
/// Validates the options provided against the map.
/// </summary>
static class Validator {

    /// <summary>
    /// Validates the command line options.
    /// </summary>
    /// <param name="map">Options map.</param>
    /// <param name="current">Current options.</param>
    /// <param name="undefined">Undefined options.</param>
    /// <param name="parametersCount">Number of parameters.</param>
    /// <param name="parametersMin">Minimal number of accepted parameters not related to an option.</param>
    /// <param name="parametersMax">Maximal number of accepted parameters not related to an option.</param>
    /// <param name="syntax">Syntax guidelines.</param>
    /// <param name="localizationProvider">
    /// A function returning localized string using the key, or null if it is not found.
    /// </param>
    /// <returns>Detailed validation error descriptions.</returns>
    public static IEnumerable<string> Validate(
        this IEnumerable<OptionMetadata> map,
        IEnumerable<Enum> current,
        IEnumerable<string>? undefined,
        int parametersCount,
        int parametersMin,
        int? parametersMax,
        ArgumentSyntax syntax,
        Func<string, string?>? localizationProvider
        ) {
        Validate(
            map,
            current,
            out var errorData
        );
        string getErrorDescriptionForOptions(string error, Enum[]? options) {
            var stringBuilder = new StringBuilder();
            void indent(int indentationLevel) => stringBuilder.Append(string.Empty.PadRight(indentationLevel));
            indent(2);
            stringBuilder.Append(Format.LocalizedString(localizationProvider, error, ValidationErrors.GetByName(error)));
            stringBuilder.AppendLine(":");
            var definitions = map.Where(d => options?.Contains(d.Option) == true);
            foreach (var definition in definitions) {
                indent(4);
                stringBuilder.AppendLine(definition.ToString(syntax));
            }
            return stringBuilder.ToString();
        }
        string getErrorDescriptionForUndefined(string error, IEnumerable<string> options) {
            var stringBuilder = new StringBuilder();
            void indent(int indentationLevel) => stringBuilder.Append(string.Empty.PadRight(indentationLevel));
            indent(2);
            stringBuilder.Append(Format.LocalizedString(localizationProvider, error, ValidationErrors.GetByName(error)));
            stringBuilder.AppendLine(":");
            indent(4);
            stringBuilder.AppendLine(
                string.Join(
                    ", ",
                    options.Select(e => Format.ToString(e, null, syntax))
                )
            );
            return stringBuilder.ToString();
        }
        string getErrorDescriptionForParameters(string error, int pLimit, int pCount) {
            var stringBuilder = new StringBuilder();
            void indent(int indentationLevel) => stringBuilder.Append(string.Empty.PadRight(indentationLevel));
            var text = Format.LocalizedString(localizationProvider, error, ValidationErrors.GetByName(error));
            text = string.Format(text, pLimit, pCount);
            indent(2);
            stringBuilder.Append(text);
            stringBuilder.AppendLine(".");
            return stringBuilder.ToString();
        }
        if (parametersCount < parametersMin)
            yield return
                getErrorDescriptionForParameters(
                    nameof(ValidationErrors.NotEnoughParameters),
                    parametersMin,
                    parametersCount
                );
        else if (parametersMax is not null && parametersCount > parametersMax)
            yield return
                getErrorDescriptionForParameters(
                    nameof(ValidationErrors.TooManyParameters),
                    parametersMax.Value,
                    parametersCount
                );
        foreach (var error in errorData)
            yield return
                getErrorDescriptionForOptions(
                    error.Key,
                    error.Value
                );
        if (undefined is not null && undefined.Any())
            yield return
                getErrorDescriptionForUndefined(
                    nameof(ValidationErrors.InvalidOption),
                    undefined
                );
    }

    /// <summary>
    /// Validates the command line options against <see cref="OptionRequired"/> rules.
    /// </summary>
    /// <param name="map">Options map.</param>
    /// <param name="current">Current options.</param>
    /// <param name="errors">Dictionary with a key being the property of the <see cref="ValidationErrors"/> class and a value containing options related.</param>
    private static void Validate(
        this IEnumerable<OptionMetadata> map,
        IEnumerable<Enum> current,
        out Dictionary<string, Enum[]?> errors
        ) {
        errors = [];
        // One missing
        var oneRequired = map.Where(e => e.Required == OptionRequired.One).Select(e => e.Option);
        if (oneRequired.Any()) {
            var n = current.Intersect(oneRequired).Count();
            if (n < 1) errors[nameof(ValidationErrors.OneMissing)] = oneRequired.ToArray();
            if (n > 1) errors[nameof(ValidationErrors.OneExceeded)] = oneRequired.ToArray();
        }
        // Any missing
        var anyRequired = map.Where(e => e.Required == OptionRequired.Any).Select(e => e.Option);
        if (anyRequired.Any()) {
            var n = current.Intersect(anyRequired).Count();
            if (n < 1) errors[nameof(ValidationErrors.AnyMissing)] = anyRequired.ToArray();
        }
        // All missing
        var allRequired = map.Where(e => e.Required == OptionRequired.All).Select(e => e.Option);
        if (allRequired.Any()) {
            var n = current.Intersect(allRequired).Count();
            if (n < allRequired.Count()) errors[nameof(ValidationErrors.AllMissing)] = allRequired.ToArray();
        }
    }

}
