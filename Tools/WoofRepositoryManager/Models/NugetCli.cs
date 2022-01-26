namespace WoofRepositoryManager.Models;

/// <summary>
/// Contains NuGet CLI helpers.
/// </summary>
public static class NugetCli {

    #region Configuration

    /// <summary>
    /// Gets the solution's root path.
    /// </summary>
    private static string Root => Path.GetFullPath(Settings.Default.Paths.Root);

    /// <summary>
    /// Gets the directory where the packages for the repository are built.
    /// </summary>
    private static string Source => Path.Combine(Root, Settings.Default.Paths.PackageBinaries);

    /// <summary>
    /// Gets the local NuGet repository.
    /// </summary>
    private static string Target => Path.Combine(Root, Settings.Default.Paths.Repo);

    #endregion

    #region API

    /// <summary>
    /// Creates or updates the local NuGet package repository.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the repository is created or updated.</returns>
    public static async ValueTask UpdateRepositoryAsync() {
        Directory.CreateDirectory(Target);
        var command = new ShellCommand($"nuget init \"{Source}\" \"{Target}\"");
        await EnsureAvailableAsync();
        await command.ExecVoidAsync();
    }

    /// <summary>
    /// Resets (deletes) the local NuGet package repository.
    /// </summary>
    public static void ResetRepository() {
        Directory.Delete(Target, recursive: true);
        Directory.CreateDirectory(Target);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Ensures the NuGet CLI tool is available. If not, it's downloaded.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when nuget command is tested or downloaded.</returns>
    private static async ValueTask EnsureAvailableAsync() {
        var nuget = new ShellCommand("nuget");
        try {
            await nuget.ExecVoidAsync();
        }
        catch {
            await DownloadAsync();
        }
    }

    /// <summary>
    /// Downloads the NuGet CLI tool.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the nuget command is downloaded.</returns>
    private static async ValueTask DownloadAsync() {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(DownloadLink);
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream("nuget.exe", FileMode.Create, FileAccess.Write, FileShare.None);
        await responseStream.CopyToAsync(fileStream);
    }

    #endregion

    #region Data / initialization

    /// <summary>
    /// Throws if settings not loaded.
    /// </summary>
    static NugetCli() => Settings.Default.Assert();

    /// <summary>
    /// NuGet.exe download link.
    /// </summary>
    private const string DownloadLink = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";

    #endregion

}