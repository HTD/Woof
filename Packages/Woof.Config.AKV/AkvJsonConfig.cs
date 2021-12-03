namespace Woof.Config;

/// <summary>
/// JSON file <see cref="IConfiguration"/> with Azure Key Vault access.
/// </summary>
public class AkvJsonConfig : JsonConfig {

    /// <summary>
    /// Gets the Azure Key Vault access provider.
    /// </summary>
    public AkvAccessProvider Access { get; }

    /// <summary>
    /// Creates a new instance of <see cref="AkvJsonConfig"/>.
    /// </summary>
    public AkvJsonConfig() : base()
        => Access = new AkvAccessProvider(new JsonConfig($"{Application.Name}.access").Get<VaultConfiguration>());

    /// <summary>
    /// Creates a new instance of <see cref="AkvJsonConfig"/> from a preloaded <see cref="JsonConfig"/>.
    /// </summary>
    /// <param name="accessConfiguration">Preloaded access configuration.</param>
    protected AkvJsonConfig(JsonConfig accessConfiguration) : base()
        => Access = new AkvAccessProvider(accessConfiguration.Get<VaultConfiguration>());

    /// <summary>
    /// Binds the read settings and sets the matching configuration values from Azure Key Vault secrets.
    /// </summary>
    /// <remarks>
    /// If the <typeparamref name="TConfiguration"/> contains a property of type <see cref="AkvAccessProvider"/>
    /// it will be set to the configured access provider.
    /// </remarks>
    /// <typeparam name="TConfiguration">The type of the target configuration record.</typeparam>
    public TConfiguration Resolve<TConfiguration>() where TConfiguration : new() {
        var configuration = this.Get<TConfiguration>();
        var properties = typeof(TConfiguration).GetProperties()
            .Select<PropertyInfo, (string name, PropertyInfo value)?>(
                p => p.GetCustomAttribute<AKVAttribute>() is AKVAttribute akv ? (akv.Name, p) : null
            )
            .OfType<(string name, PropertyInfo value)>();
        foreach (var property in properties) {
            object? value = property.value.PropertyType.Name switch {
                "String" => Access.GetString(property.name),
                "Byte[]" => Access.GetBytes(property.name),
                _ => null
            };
            if (value is not null) property.value.SetValue(configuration, value);
        }
        var accessProviderProperty = typeof(TConfiguration).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(AkvAccessProvider));
        if (accessProviderProperty is not null) accessProviderProperty.SetValue(configuration, Access);
        return configuration;
    }

}