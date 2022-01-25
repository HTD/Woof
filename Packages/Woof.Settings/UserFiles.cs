namespace Woof.Settings;

/// <summary>
/// Gets the directories suitable for storing user files depending on the current operating system.
/// </summary>
public static class UserFiles {

    /// <summary>
    /// Gets the home directory like "~/.[app]" for the curren Linux user.
    /// </summary>
    public static DirectoryInfo HomeDirectory
        => _HomeDirectory ??=
            new (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), '.' + Executable.FileName));

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
    public static DirectoryInfo LocalAppDataDirectory {
        get {
            if (_LocalAppDataDirectory is not null) return _LocalAppDataDirectory;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var executable = Executable.Assembly;
            var company = executable.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            var product = executable.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            var path =
                company is not null && product is not null && company != product ? Path.Combine(localAppData, company, product) :
                product is not null ? Path.Combine(localAppData, product) :
                Path.Combine(localAppData, Executable.FileName);
            return _LocalAppDataDirectory = new(path);
        }
    }

    /// <summary>
    /// Gets the target directory for user files, platform dependent.<br/>
    /// For Windows it will be located in %LOCALAPPDATA%, for Linux - in ~.<br/>
    /// See <see cref="LocalAppDataDirectory"/> and <see cref="HomeDirectory"/> documentation.
    /// </summary>
    public static DirectoryInfo UserDirectory
        => _UserDirectory ??= RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LocalAppDataDirectory : HomeDirectory;

    #region Backing fields

    private static DirectoryInfo? _HomeDirectory;
    private static DirectoryInfo? _LocalAppDataDirectory;
    private static DirectoryInfo? _UserDirectory;

    #endregion

}