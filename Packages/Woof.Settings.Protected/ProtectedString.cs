namespace Woof.Settings.Protected;

/// <summary>
/// Represents a special, optionally protected string.
/// </summary>
/// <remarks>
/// Used to store some configuration string that should be protected for the default protection scope.
/// The user can store either protected or unprotected string.
/// </remarks>
public class ProtectedString : ISerializableSettingsValue, IProtectedSettingsValue {

    /// <summary>
    /// Gets a value indicating that the string is protected.
    /// </summary>
    public bool IsProtected => Serialized.Length > 1 && Serialized[0..2] == ProtectedData.LockSymbol;

    /// <summary>
    /// Gets or sets the unprotected string.
    /// </summary>
    public string Value {
        get {
            if (Serialized.Length < 1 || !IsProtected) return Serialized;
            try {
                return Encoding.UTF8.GetString(DP.Unprotect(Convert.FromBase64String(Serialized[2..]), ProtectedData.DefaultDataProtectionScope));
            }
            catch {
                return string.Empty;
            }
        }
        set => Serialized = value;
    }

    /// <summary>
    /// Protects the serialized data.
    /// </summary>
    public void Protect() {
        if (Serialized is null || Serialized.Length < 1 || IsProtected) return;
        var data = Encoding.UTF8.GetBytes(Serialized);
        Serialized = ProtectedData.LockSymbol + Convert.ToBase64String(DP.Protect(data, ProtectedData.DefaultDataProtectionScope));
    }

    /// <summary>
    /// Returns the serialized value.
    /// </summary>
    /// <returns>Serialized value.</returns>
    public string GetString() => Serialized;

    /// <summary>
    /// Implements <see cref="ISerializableSettingsValue"/>.
    /// </summary>
    /// <param name="value">Value to parse.</param>
    /// <returns>This instance.</returns>
    public void Parse(string value) => Serialized = value;

    /// <summary>
    /// Stores the serialized, protected value.
    /// </summary>
    private string Serialized = string.Empty;

}