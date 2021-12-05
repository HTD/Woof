namespace Woof.Config;

/// <summary>
/// JSON file <see cref="IConfiguration"/>.
/// </summary>
public class JsonConfig : IConfiguration {

    /// <summary>
    /// Gets a value indicating that the calling assembly was built with the Debug configuration.
    /// </summary>
    public static bool IsDebug { get; }

    /// <summary>
    /// Gets the configuration path.
    /// </summary>
    public string SourcePath { get; protected set; }

    /// <summary>
    /// Gets the target directory for user files, platform dependent.<br/>
    /// For Windows it will be located in %LOCALAPPDATA%, for Linux - in ~.<br/>
    /// See <see cref="LocalAppDataTarget"/> and <see cref="HomeDirectoryTarget"/> documentation.
    /// </summary>
    public static string UserTarget =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LocalAppDataTarget :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? HomeDirectoryTarget :
        throw new PlatformNotSupportedException();

    /// <summary>
    /// Gets the %LOCALAPPDATA% target folder for the current Windows user.
    /// </summary>
    /// <remarks>
    /// In order for for this to work with Microsoft Installer Projects,<br/>
    /// the installer must put the the program files in [ProgramFiles64Folder]\[Manufacturer]\[ProductName]<br/>
    /// and the configuration files in [LocalAppDataFolder]\[Manufacturer]\[ProductName].<br/><br/>
    /// The installer folders are matched as follows:<br/>
    /// - [LocalAppDataFolder] => %LOCALAPPDATA%,<br/>
    /// - [Manufacturer] => <see cref="AssemblyCompanyAttribute.Company"/> property,<br/>
    /// - [ProductName] => <see cref="AssemblyProductAttribute.Product"/> property.<br/>
    /// When the assembly attributes are not set, following fallback paths are used:<br/>
    /// - [LocalAppDataFolder]\[ProductName] - when the Company attribute is not set or is equal to Product attribute value,<br/>
    /// - [LocalAppDataFolder]\[ApplicationName] - when no assembly attributes are set.<br/>
    /// </remarks>
    public static string LocalAppDataTarget {
        get {
            if (_LocalAppDataTarget is not null) return _LocalAppDataTarget;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var executable = Assembly.GetEntryAssembly()!;
            var company = executable.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            var product = executable.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            return _LocalAppDataTarget =
                company is not null && product is not null && company != product ? SysPath.Combine(localAppData, company, product) :
                product is not null ? SysPath.Combine(localAppData, product) :
                SysPath.Combine(localAppData, Application.Name);
        }
    }

    /// <summary>
    /// Gets the home directory target like "~/.[app]" for the curren Linux user.
    /// </summary>
    public static string HomeDirectoryTarget
        => _UserHomeDirectoryTarget ??=
            SysPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), '.' + Application.Name);

    /// <summary>
    /// Gets the <see cref="JsonNodeSection"/> instance.
    /// </summary>
    protected virtual JsonNodeSection Configuration { get; }

    /// <summary>
    /// Gets the supported configuration file extensions. Derived class can add its own.
    /// </summary>
    protected virtual IEnumerable<string> Extensions => new[] { ".json" };

    /// <summary>
    /// Gets the optional loading flag that can be set when the configuration is loaded by the derived class.
    /// </summary>
    protected bool LoadingFlag { get; }

    /// <summary>
    /// Creates the configuration for the application name.
    /// </summary>
    /// <remarks>
    /// A matching configuration file must exist either in the output directory or in the user's target directory.<br/>
    /// See <see cref="LocalAppDataTarget"/> and <see cref="HomeDirectoryTarget"/> documentation.<br/>
    /// For DEBUG configuration, a file "[ApplicationName].dev.json" will be used if present.
    /// </remarks>
    public JsonConfig() : this(Application.Name) { }

    /// <summary>
    /// Creates the configuration for the specified name.
    /// </summary>
    /// <param name="name">Name of the configuration. No extension! ".json" will be added.</param>
    /// <remarks>
    /// A matching configuration file must exist either in the output directory or in the user's target directory.<br/>
    /// See <see cref="LocalAppDataTarget"/> and <see cref="HomeDirectoryTarget"/> documentation.<br/>
    /// For DEBUG configuration, a file "[ApplicationName].dev.json" will be used if present.
    /// </remarks>
    public JsonConfig(string name) {
        SourcePath = GetPath(name);
        Configuration = JsonNodeSection.Loader.Load(GetReadStream(SourcePath));
    }

    /// <summary>
    /// Creates the configuration for the specified name with the custom loader provide by derived class.
    /// </summary>
    /// <param name="loadFromPath">
    /// A function accepting the configuration file path,
    /// returning the tuple consisting of the decoded configuration node and a boolean flag passed to derived class.
    /// </param>
    protected JsonConfig(Func<string, (JsonNodeSection configuration, bool flag)> loadFromPath) {
        SourcePath = GetPath(Application.Name);
        var (configuration, flag) = loadFromPath(SourcePath);
        Configuration = configuration;
        LoadingFlag = flag;
    }

    /// <summary>
    /// Creates the configuration for the specified name with the custom loader provide by derived class.
    /// </summary>
    /// <param name="name">Name of the configuration. No extension!</param>
    /// <param name="loadFromPath">A function accepting the configuration file path and returning the decoded configuration node.</param>
    protected JsonConfig(string name, Func<string, (JsonNodeSection configuration, bool flag)> loadFromPath) {
        SourcePath = GetPath(name);
        Configuration = loadFromPath(SourcePath).configuration;
    }

    /// <summary>
    /// Saves the current state of the configuration to the original JSON file.
    /// </summary>
    /// <remarks>
    /// This will work only if the configuration file directory is writeable to the user and the configuration doesn't contain nullable values.
    /// </remarks>
    public virtual void Save() {
        using var writeStream = GetWriteStream(SourcePath);
        Configuration.Save(writeStream);
    }

    /// <summary>
    /// Saves the current state of the configuration to the original JSON file.
    /// </summary>
    /// <remarks>
    /// This will work only if the configuration file directory is writeable to the user and the configuration doesn't contain nullable values.
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> completed when the configuration is saved.</returns>
    public virtual async ValueTask SaveAsync() {
        await using var writeStream = GetWriteStream(SourcePath);
        await Configuration.SaveAsync(writeStream);
    }

    /// <summary>
    /// Converts the configuration to <see cref="JsonNodeSection"/> type.
    /// </summary>
    /// <param name="config">Converted value.</param>
    public static implicit operator JsonNodeSection(JsonConfig config) => config.Configuration;

    #region Implementation

    /// <summary>
    /// Determines main executable build configuration.
    /// </summary>
    static JsonConfig()
        => IsDebug = Assembly.GetEntryAssembly()!.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);

    /// <summary>
    /// Gets the configuration file path.
    /// </summary>
    /// <param name="name">Configuration name.</param>
    /// <param name="preferUserDirectory">Set to search the user directory first.</param>
    /// <returns>Full path to the matching configuration file.</returns>
    /// <exception cref="FileNotFoundException">Can't find the configuration file.</exception>
    private string GetPath(string name, bool preferUserDirectory = false) {
        var programDirectory = Application.Directory;
        var userDirectory = UserTarget;
        var targets = preferUserDirectory
            ? new[] { userDirectory, programDirectory }
            : new[] { programDirectory, userDirectory };
        IEnumerable<string> getDebugExtensions() { // for debug configuration ".dev" files should be matched first!
            foreach (var extension in Extensions) {
                yield return ".dev" + extension;
                yield return extension;
            }
        }
        var extensions = IsDebug ? getDebugExtensions() : Extensions;
        string candidate;
        foreach (var target in targets) {
            foreach (var extension in extensions) {
                candidate = SysPath.Combine(target, name + extension);
                if (File.Exists(candidate)) return candidate;
            }
        }
        throw new FileNotFoundException($"Missing configuration file for {name}");
    }

    /// <summary>
    /// Gets a file stream for reading.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>Read stream.</returns>
    private static Stream GetReadStream(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    /// <summary>
    /// Gets a file stream for writing.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>Write stream.</returns>
    private static Stream GetWriteStream(string path) => new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

    #region IConfiguration implementation

    /// <summary>
    /// Gets or sets a configuration value.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value.</returns>
    /// <exception cref="NotImplementedException">Set a non-existing key.</exception>
    public string this[string key] { get => ((IConfiguration)Configuration)[key]; set => ((IConfiguration)Configuration)[key] = value; }

    /// <summary>
    /// Gets a configuration sub-section with the specified key.
    /// </summary>
    /// <param name="key">The key of the configuration section.</param>
    /// <returns>The Microsoft.Extensions.Configuration.IConfigurationSection.</returns>
    /// <remarks>
    /// This method will never return null. If no matching sub-section is found with
    /// the specified key, an empty Microsoft.Extensions.Configuration.IConfigurationSection
    /// will be returned.
    /// </remarks>
    public IConfigurationSection GetSection(string key) => Configuration.GetSection(key);

    /// <summary>
    /// Gets the immediate descendant configuration sub-sections.
    /// </summary>
    /// <returns>The configuration sub-sections.</returns>
    public IEnumerable<IConfigurationSection> GetChildren() => Configuration.GetChildren();

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Exception.</returns>
    /// <exception cref="NotImplementedException">Invoked.</exception>
    public IChangeToken GetReloadToken() => Configuration.GetReloadToken();

    #endregion

    /// <summary>
    /// <see cref="LocalAppDataTarget"/> backing field.
    /// </summary>
    private static string? _LocalAppDataTarget;

    /// <summary>
    /// <see cref="HomeDirectoryTarget"/> backing field.
    /// </summary>
    private static string? _UserHomeDirectoryTarget;

    #endregion

}