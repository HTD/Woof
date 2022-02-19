namespace Woof.DataProtection;

/// <summary>
/// Contains data protection key configuration data.
/// </summary>
public record struct DataProtectionKeyConfiguration(
    IDataProtector Protector,
    IDataProtectionProvider Provider,
    IKey Key,
    IKeyManager KeyManager,
    DirectoryInfo KeyDirectory,
    FileInfo KeyFile
);