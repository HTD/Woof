using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Woof.DataProtection;

/// <summary>
/// Implementing class contains complete data protection key details.
/// </summary>
public interface IDataProtectionKey {

    /// <summary>
    /// Gets the data protector instance.
    /// </summary>
    IDataProtector Protector { get; }

    /// <summary>
    /// Gets the <see cref="IDataProtectionProvider"/> instance.
    /// </summary>
    IDataProtectionProvider Provider { get; }

    /// <summary>
    /// Gets the <see cref="IKey"/> instance.
    /// </summary>
    IKey Key { get; }

    /// <summary>
    /// Gets the <see cref="IKeyManager"/> instance.
    /// </summary>
    IKeyManager KeyManager { get; }

    /// <summary>
    /// Gets the key directory.
    /// </summary>
    DirectoryInfo KeyDirectory { get; }

    /// <summary>
    /// Gets the full key file path.
    /// </summary>
    FileInfo KeyFile { get; }

}