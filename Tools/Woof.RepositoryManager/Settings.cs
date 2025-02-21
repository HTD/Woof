using Woof.Settings.Protected;

namespace Woof.RepositoryManager;

#pragma warning disable CS8618 // no null values when the settings are loaded, without settings loaded.
/// <summary>
/// Application settings to be loaded on start.
/// </summary>
public class Settings : JsonSettings<Settings> {

    /// <summary>
    /// Gets the singleton instance of the settings.
    /// </summary>
    public static Settings Default { get; } = new Settings();

    /// <summary>
    /// Gets a value indicating that the configuration is loaded and all required properties are set.
    /// </summary>
    [Internal]
    public bool OK
        => IsLoaded &&
        DotNetFrameworkName is not null &&
        Paths.PackageBinaries is not null && Paths.Repo is not null && Paths.Root is not null;

    /// <summary>
    /// Gets the settings file path.
    /// </summary>
    [Internal]
    public string File => _Metadata.FilePath!;

    /// <summary>
    /// Editor command.
    /// </summary>
    public string? Editor { get; init; }

    /// <summary>
    /// Gets the toolkit's paths.
    /// </summary>
    public PathsData Paths { get; } = new();

    /// <summary>
    /// Gets the NuGet prefix for the toolkit.
    /// </summary>
    public string Prefix { get; init; }

    /// <summary>
    /// Gets the framework name string for package fitlering.
    /// </summary>
    public string DotNetFrameworkName { get; init; }

    /// <summary>
    /// Gets the feed targets.
    /// </summary>
    public NuGetFeed[] Feeds { get; init; }

    /// <summary>
    /// Defines the paths used by the application.
    /// </summary>
    public record PathsData {

        /// <summary>
        /// Gets the solution root.
        /// </summary>
        public string Root { get; init; }

        /// <summary>
        /// Gets the local NuGet repository.
        /// </summary>
        public string Repo { get; init; }

        /// <summary>
        /// Gets the directory where the packages for the repository are built.
        /// </summary>
        public string PackageBinaries { get; init; }

    }

    /// <summary>
    /// Defines a NuGet feed.
    /// </summary>
    public record NuGetFeed {

        /// <summary>
        /// Gets the display name of the feed.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the URI of the feed.
        /// </summary>
        public Uri Uri { get; init; }

        /// <summary>
        /// Gets the API key if configured.
        /// </summary>
        public ProtectedString? ApiKey { get; set; }

    }

    private Settings() => _Metadata.Locator.PreferUserDirectory = true;

    /// <summary>
    /// Loads the settings and saves the protected version if there are any unprotected API keys in it.
    /// </summary>
    /// <returns>Settings instance.</returns>
    public override Settings Load() {
        base.Load();
        if (Feeds.Any(feed => feed.ApiKey?.Value.Length > 0 && !feed.ApiKey.IsProtected)) Save();
        return this;
    }

    /// <summary>
    /// Loads the settings and saves the protected version if there are any unprotected API keys in it.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning settings instance.</returns>
    public override async ValueTask<Settings> LoadAsync() {
        await base.LoadAsync();
        if (Feeds.Any(feed => feed.ApiKey?.Value.Length > 0 && !feed.ApiKey.IsProtected)) await SaveAsync();
        return this;
    }

}
#pragma warning restore CS8618