namespace Woof.Config.Protected;

/// <summary>
/// Data-protected JSON IConfiguration.
/// </summary>
public sealed class JsonConfigProtected : JsonConfig {

    /// <summary>
    /// Gets the supported configuration file extensions.
    /// </summary>
    protected override IEnumerable<string> Extensions => base.Extensions.Append(Ext);

    /// <summary>
    /// Gets a value indicating that the configuration file is protected.
    /// </summary>
    public bool IsProtected { get; private set; }

    /// <summary>
    /// Gets the data protecton scope.
    /// </summary>
    public DataProtectionScope Scope { get; }

    /// <summary>
    /// Creates the configuration for the application name.
    /// </summary>
    /// <param name="scope">Data protection scope, default: <see cref="DataProtectionScope.CurrentUser"/>.</param>
    /// <remarks>
    /// A matching configuration file must exist either in the output directory or in the user's target directory.<br/>
    /// See <see cref="JsonConfig.LocalAppDataTarget"/> and <see cref="JsonConfig.HomeDirectoryTarget"/> documentation.<br/>
    /// For DEBUG configuration, a file "[ApplicationName].dev.json" will be used if present.
    /// </remarks>
    public JsonConfigProtected(DataProtectionScope scope = default)
        : base(path => GetConfigurationFromPath(path, scope)) {
        IsProtected = LoadingFlag;
        Scope = scope;
    }

    /// <summary>
    /// Creates the configuration for the specified name.
    /// </summary>
    /// <param name="name">Name of the configuration. No extension! ".json" will be added.</param>
    /// <param name="scope">Data protection scope, default: <see cref="DataProtectionScope.CurrentUser"/>.</param>
    /// <remarks>
    /// A matching configuration file must exist either in the output directory or in the user's target directory.<br/>
    /// See <see cref="JsonConfig.LocalAppDataTarget"/> and <see cref="JsonConfig.HomeDirectoryTarget"/> documentation.<br/>
    /// For DEBUG configuration, a file "[ApplicationName].dev.json" will be used if present.
    /// </remarks>
    public JsonConfigProtected(string name, DataProtectionScope scope = default)
        : base(name, path => GetConfigurationFromPath(path, scope)) {
        IsProtected = LoadingFlag;
        Scope = scope;
    }

    /// <summary>
    /// Encrypts the configuration file. Deletes the original one. If the file is already protected this call is ignored.
    /// </summary>
    /// <returns>This configuration.</returns>
    public JsonConfigProtected Protect() {
        if (IsProtected) return this;
        if (!DP.IsAvailable) return this;
        string? unprotectedFile = null;
        if (Path.GetExtension(SourcePath) != Ext) {
            unprotectedFile = SourcePath;
            var dir = Path.GetDirectoryName(SourcePath);
            var name = Path.GetFileNameWithoutExtension(SourcePath) + Ext;
            SourcePath = dir is null ? name : Path.Combine(dir, name);
        }
        if (!SaveProtected()) return this;
        if (unprotectedFile is not null) File.Delete(unprotectedFile);
        IsProtected = true;
        return this;
    }

    /// <summary>
    /// Saves the current state of the configuration to the original JSON file.
    /// </summary>
    /// <remarks>
    /// This will work only if the configuration file directory is writeable to the user and the configuration doesn't contain nullable values.
    /// </remarks>
    public override void Save() {
        if (!IsProtected) { base.Save(); return; }
        if (!DP.IsAvailable) throw new NotSupportedException(DPAPI_Error);
        SaveProtected();
    }

    /// <summary>
    /// Saves the current state of the configuration to the original JSON file.
    /// </summary>
    /// <remarks>
    /// This will work only if the configuration file directory is writeable to the user and the configuration doesn't contain nullable values.
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> completed when the configuration is saved.</returns>
    public override async ValueTask SaveAsync() {
        if (!IsProtected) await base.SaveAsync();
        if (!DP.IsAvailable) throw new NotSupportedException(DPAPI_Error);
        await SaveProtectedAsync();
    }

    /// <summary>
    /// Gets the configuration from a file.
    /// </summary>
    /// <param name="path">Full path to the file.</param>
    /// <param name="scope">Data protection scope.</param>
    /// <returns>A tuple consisting of the configuration root node and a bolean value true if the configuration was already protected.</returns>
    /// <exception cref="NotSupportedException">Data protection API is not available.</exception>
    private static (JsonNodeConfiguration, bool) GetConfigurationFromPath(string path, DataProtectionScope scope) {
        if (Path.GetExtension(path) != Ext) return (JsonNodeConfiguration.Load(path), false);
        if (!DP.IsAvailable) throw new NotSupportedException(DPAPI_Error);
        using var readStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var protectedData = new byte[readStream.Length];
        readStream.Read(protectedData, 0, protectedData.Length);
        var jsonText = Encoding.UTF8.GetString(DP.Unprotect(protectedData, scope));
        return (new JsonNodeConfiguration(jsonText), true);
    }

    /// <summary>
    /// Saves the configuration to the file as protected.
    /// </summary>
    /// <returns>True if there was anything to write.</returns>
    private bool SaveProtected() {
        var jsonText = Configuration.Node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        if (jsonText is null) return false;
        var jsonDataRaw = Encoding.UTF8.GetBytes(jsonText);
        var jsonDataProtected = DP.Protect(jsonDataRaw, Scope);
        using var writeStream = new FileStream(SourcePath, FileMode.Create, FileAccess.Write, FileShare.None);
        writeStream.Write(jsonDataProtected, 0, jsonDataProtected.Length);
        return true;
    }

    /// <summary>
    /// Saves the configuration to the file as protected.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning true if there was anything to write.</returns>
    private async ValueTask<bool> SaveProtectedAsync() {
        var jsonText = Configuration.Node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        if (jsonText is null) return false;
        var jsonDataRaw = Encoding.UTF8.GetBytes(jsonText);
        var jsonDataProtected = DP.Protect(jsonDataRaw, Scope);
        await using var writeStream = new FileStream(SourcePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await writeStream.WriteAsync(jsonDataProtected.AsMemory());
        return true;
    }

    /// <summary>
    /// Protected file extension.
    /// </summary>
    private const string Ext = ".data";

    /// <summary>
    /// An error message for the data protection API not available.
    /// </summary>
    private const string DPAPI_Error = "Data protection API is not available";

}