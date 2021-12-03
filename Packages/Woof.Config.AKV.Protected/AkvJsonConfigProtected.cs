namespace Woof.Config.AKV.Protected;

/// <summary>
/// JSON file <see cref="IConfiguration"/> with Azure Key Vault access and access data protected using data protection API.
/// </summary>
public class AkvJsonConfigProtected : AkvJsonConfig {

    /// <summary>
    /// Creates a new instance of <see cref="AkvJsonConfigProtected"/> with protected vault access configuration.
    /// </summary>
    /// <param name="scope">Data protection scope.</param>
    public AkvJsonConfigProtected(DataProtectionScope scope = default)
        : base(new JsonConfigProtected($"{Application.Name}.access", scope).Protect()) { }

}