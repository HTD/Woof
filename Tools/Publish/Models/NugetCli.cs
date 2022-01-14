namespace Publish.Models;

public class NugetCli {

    public const string DownloadLink = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";

    public static async ValueTask ReloadRepositoryAsync() {
        var root = Path.GetFullPath(Settings.Default.Paths.Root);
        var source = Path.Combine(root, Settings.Default.Paths.PackageBinaries);
        var target = Path.Combine(root, Settings.Default.Paths.Repo);
        var command = new ShellCommand($"nuget init \"{source}\" \"{target}\"");
        await EnsureAvailableAsync();
        await command.ExecVoidAsync();
    }

    private static async ValueTask EnsureAvailableAsync() {
        var nuget = new ShellCommand("nuget");
        try {
            await nuget.ExecVoidAsync();
        }
        catch {
            await DownloadAsync();
        }
    }

    private static async ValueTask DownloadAsync() {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(DownloadLink);
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream("nuget.exe", FileMode.Create, FileAccess.Write, FileShare.None);
        await responseStream.CopyToAsync(fileStream);
    }


}
