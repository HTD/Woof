namespace Woof.Internals.CommandLine;

/// <summary>
/// Contains command line option metadata.
/// </summary>
class OptionMetadata : IOptionMetadata {

    /// <summary>
    /// Gets or sets the option enumeration member.
    /// </summary>
    public Enum Option { get; set; } = null!;

    /// <summary>
    /// Gets or sets the option aliases separated with pipe symbol.
    /// </summary>
    public string Aliases { get; set; } = null!;

    /// <summary>
    /// Gets or sets the optional value placeholder name.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional option description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets how the option is required in the command line.
    /// </summary>
    public OptionRequired Required { get; set; }

    /// <summary>
    /// Gets the option metadata collection from the configured enumeration type.
    /// </summary>
    /// <typeparam name="TEnum">Configured enumeration type.</typeparam>
    /// <returns>Option metadata collection.</returns>
    public static IEnumerable<OptionMetadata> Get<TEnum>() where TEnum : struct, Enum {
        var values = Enum.GetValues<TEnum>();
        foreach (var e in values) {
            var name = e.ToString();
            var enumType = e.GetType();
            var memberInfo = enumType
                .GetTypeInfo()
                .GetMember(name)?
                .First(member => member.MemberType == MemberTypes.Field);
            var optionAttribute = memberInfo?.GetCustomAttribute<OptionAttribute>(inherit: false);
            if (optionAttribute is null) continue;
            if (optionAttribute.OSPlatform is string s && !RuntimeInformation.IsOSPlatform(OSPlatform.Create(s))) continue;
            yield return new OptionMetadata {
                Option = (Enum)Enum.Parse(enumType, name),
                Aliases = optionAttribute.Aliases,
                Value = optionAttribute.Value,
                Description = optionAttribute.Description,
                Required = optionAttribute.Required
            };
        }
    }

    /// <summary>
    /// Converts option metadata to the underlying enumeration member instance.
    /// </summary>
    /// <param name="metadata">Option metadata.</param>
    public static implicit operator Enum(OptionMetadata metadata) => metadata.Option;

}
