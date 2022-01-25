namespace Woof.Settings.Protected;

/// <summary>
/// Represents a special, optionally protected byte array.
/// </summary>
/// <remarks>
/// Used to store some configuration data that should be protected for the default protection scope.
/// The user can store either protected or unprotected data.
/// </remarks>
public class ProtectedData : ISerializableSettingsValue, IProtectedSettingsValue {

    /// <summary>
    /// Gets or sets the data protection scope for the instances. Default is <see cref="DataProtectionScope.CurrentUser"/>.
    /// </summary>
    public static DataProtectionScope DefaultDataProtectionScope { get; set; }

    /// <summary>
    /// Gets a value indicating that the data is protected.
    /// </summary>
    public bool IsProtected => Serialized?.Length > 1 && Serialized[0..2] == "🐕"; // the Unicode dogo is the data protection ID sequence.

    /// <summary>
    /// Gets or sets the unprotected data.
    /// </summary>
    public byte[] Value {
        get {
            if (Serialized is null) return Array.Empty<byte>();
            if (!IsProtected) return Convert.FromBase64String(Serialized);
            try {
                return DP.Unprotect(Convert.FromBase64String(Serialized[2..]), DefaultDataProtectionScope);
            }
            catch {
                return Array.Empty<byte>();
            }
        }
        set => Serialized = Convert.ToBase64String(value);
    }

    /// <summary>
    /// Protects the serialized data.
    /// </summary>
    public void Protect() {
        if (Serialized.Length < 1 || IsProtected) return;
        var data = Convert.FromBase64String(Serialized);
        Serialized = "🐕" + Convert.ToBase64String(DP.Protect(data, DefaultDataProtectionScope));
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