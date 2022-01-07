namespace Woof.Settings;

/// <summary>
/// Property marked with this attribute should be retrieved as a secret from Azure Key Vault.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AKVAttribute : SpecialAttribute {

    /// <summary>
    /// Creates a new AKV attribute with Azure Key Vault secret value name.
    /// </summary>
    /// <param name="name">Azure Key Vault secret value name.</param>
    public AKVAttribute(string name) => Name = name;

    /// <summary>
    /// Gets or sets the Azure Key Vault sercret value name.
    /// </summary>
    public string Name { get; }

}