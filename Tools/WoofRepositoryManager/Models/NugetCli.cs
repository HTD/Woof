namespace WoofRepositoryManager.Models;

/// <summary>
/// Contains NuGet CLI helpers.
/// </summary>
public static class NugetCli {

    /// <summary>
    /// Creates or reloads the local NuGet package repository.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the repository is created or updated.</returns>
    public static async ValueTask ReloadRepositoryAsync() {
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

    #region Static configuration

    /// <summary>
    /// Gets the necessary configuration values from the program settings on first access.
    /// </summary>
    static NugetCli() {
        if (!Settings.Default.IsLoaded) Settings.Default.Load();
        Root = Path.GetFullPath(Settings.Default.Paths.Root);
        Source = Path.Combine(Root, Settings.Default.Paths.PackageBinaries);
        Target = Path.Combine(Root, Settings.Default.Paths.Repo);
    }

    private const string DownloadLink = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
    private static readonly string Root;
    private static readonly string Source;
    private static readonly string Target;

    #endregion

}
