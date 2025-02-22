namespace Woof.RepositoryManager.ViewModels;

/// <summary>
/// Main view model.
/// </summary>
public class MainView : ViewModelBase, IGetAsync {

    /// <summary>
    /// Gets or sets a value indicating the view model is busy processing the data.
    /// </summary>
    public bool IsBusy {
        get => GetValue(nameof(IsBusy), false);
        set => SetValue(nameof(IsBusy), value);
    }

    /// <summary>
    /// Gets a value indicating the view data is loaded.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Gets the targets for the view.
    /// </summary>
    public ObservableList<Settings.NuGetFeed> Feeds { get; } = [];

    /// <summary>
    /// Gets or sets the feed currently selected.
    /// </summary>
    public Settings.NuGetFeed? CurrentFeed {
        get => GetValue<Settings.NuGetFeed?>(nameof(CurrentFeed), null);
        set => SetValue(nameof(CurrentFeed), value);
    }

    /// <summary>
    /// Gets the packages for the view.
    /// </summary>
    public ObservableList<PackageNode> Packages { get; } = [];

    /// <summary>
    /// Gets or sets the status text.
    /// </summary>
    public string? Status {
        get => GetValue<string>(nameof(Status), null);
        set => SetValue(nameof(Status), value);
    }

    /// <summary>
    /// Gets the view data, here - initializes the application.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the view data is loaded.</returns>
    public async ValueTask GetAsync() {
        if (!IsInitialized) await InitializeAsync();
        try {
            IsBusy = true;
            Status = "Loading local repository...";
            IsLoaded = false;
            Feeds.Clear();
            Feeds.AddRange(Settings.Default.Feeds);
            await Packages.GetAsync();
            IsLoaded = true;
        }
        catch (Exception exception) {
            Status = exception.Message;
        }
        finally {
            IsBusy = false;
            Status = null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether a command with the specified parameter can be executed.
    /// </summary>
    /// <param name="parameter">Command parameter.</param>
    /// <returns>True if command can be executed.</returns>
    public override bool CanExecute(object? parameter) => true;

    /// <summary>
    /// Executes a command with the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Command parameter.</param>
    public override async void Execute(object? parameter) {
        if (parameter is not string command) return;
        switch (command) {
            case "Update":
                Status = "Updating the local repository...";
                await CallAsync(command, NugetCli.UpdateRepositoryAsync, reloadView: true);
                Status = null;
                break;
            case "Reset":
                Status = "Resetting the local repository...";
                await CallAsync(command, NugetCli.ResetRepositoryAsync, reloadView: true);
                Status = null;
                break;
            case "Collapse":
                foreach (var package in Packages.All()) package.IsExpanded = false;
                break;
            case "Expand":
                foreach (var package in Packages.All()) package.IsExpanded = true;
                break;
            case "None":
                foreach (var package in Packages) package.IsChecked = false;
                break;
            case "All":
                foreach (var package in Packages) package.IsChecked = true;
                break;
            case "Publish":
                await CallAsync(command, async () => await PublishPackagesAsync(Packages.GetChecked()));
                break;
            case "Delete":
                await CallAsync(command, async () => await DeletePackagesAsync(Packages.GetChecked()));
                break;
            case "OpenSettings": {
                    var settingsDirectory = UserFiles.UserDirectory;
                    if (!settingsDirectory.Exists) settingsDirectory.Create();
                    if (Settings.Default.Editor is string editor) System.Diagnostics.Process.Start(editor, $"\"{Settings.Default.File}\"");
                    else {
                        var showSettingsDirectoryCommand = new ShellCommand($"\"{Settings.Default.File}\"");
                        await showSettingsDirectoryCommand.ExecAndForgetAsync();
                    }
                }
                break;
            case "ReloadSettings":
                CurrentFeed = null;
                await CallAsync(command, ReloadSettingsAsync);
                break;
        }
    }

    /// <summary>
    /// Initializes the main view.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the view is initialized.</returns>
    private async ValueTask InitializeAsync() {
        Executable.ResetAssembly<Settings>();
        await Settings.Default.LoadAsync();
        if (!Settings.Default.OK) {
            MessageBox.Show("Required settings missing", "Initialization", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            App.Current.Shutdown();
        }
        IsInitialized = true;
    }

    private async ValueTask ReloadSettingsAsync() {
        await Settings.Default.LoadAsync();
        await GetAsync();
    }

    /// <summary>
    /// Calls an asynchronous action and displays a spinner until the action completes. Reloads the view optionally.
    /// </summary>
    /// <param name="commandName">A name of the command for an error message box if the action throws.</param>
    /// <param name="asyncAction">An asynchronous action to call.</param>
    /// <param name="reloadView">True to reload the view after the action is completed.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the action completes and optionally the view is reloaded.</returns>
    private async ValueTask CallAsync(string commandName, Func<ValueTask> asyncAction, bool reloadView = false) {
        try {
            IsBusy = true;
            await asyncAction();
            if (reloadView) {
                IsLoaded = false;
                Packages.Clear();
                await Packages.GetAsync();
                IsLoaded = true;
            }
        }
        catch (Exception exception) {
            MessageBox.Show($"{commandName} failed:\n{exception.Message}", commandName, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally {
            IsBusy = false;
            Status = default;
        }
    }

    /// <summary>
    /// Calls a synchronous action and displays a spinner until the action completes. Reloads the view optionally.
    /// </summary>
    /// <param name="commandName">A name of the command for an error message box if the action throws.</param>
    /// <param name="asyncAction">A synchronous action to call.</param>
    /// <param name="reloadView">True to reload the view after the action is completed.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the action completes and optionally the view is reloaded.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Might be needed later")]
    private ValueTask CallAsync(string commandName, Action action, bool reloadView)
        => CallAsync(commandName, () => { action(); return ValueTask.CompletedTask; }, reloadView);

    /// <summary>
    /// Publishes the selected packages to the selected target feed.
    /// </summary>
    /// <param name="packages">Packages selected.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the packages are published.</returns>
    private async ValueTask PublishPackagesAsync(IEnumerable<PackageNode> packages) {
        if (!packages.Any()) return;
        foreach (var package in packages) {
            try {
                Status = $"Publishing package {package.Name} {package.Version}...";
                var packagePath = Path.Combine(LocalRepository.Path, package.Name, package.Version, $"{package.Name}.{package.Version}.nupkg");
                if (CurrentFeed is Settings.NuGetFeed feed) {
                    var commandLine = feed.ApiKey is not null && feed.ApiKey.Value.Length > 0
                        ? $"nuget push -Source {feed.Uri.OriginalString} -ApiKey {feed.ApiKey.Value} \"{packagePath}\""
                        : $"nuget push -Source {feed.Uri.OriginalString} \"{packagePath}\"";
                    var command = new ShellCommand(commandLine);
                    await command.ExecVoidAsync();
                }
                else await Task.Delay(250);
                Status += "OK";
            }
            catch {
                Status += "ERROR!";
                throw;
            }
            finally {
                package.IsChecked = false;
                foreach (var p in Packages.Where(p => p.Name == package.Name && p.Version == package.Version)) p.IsChecked = false;
            }
        }
        Status = null;
    }

    /// <summary>
    /// Deletes or unlists the selected packages from the selected target feed.
    /// </summary>
    /// <param name="packages">Packages selected.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the packages are deleted / unlisted.</returns>
    private async ValueTask DeletePackagesAsync(IEnumerable<PackageItem> packages) {
        if (!packages.Any() || CurrentFeed is null) return;
        foreach (var package in packages) {
            Status = $"Deleting package {package.Name} {package.Version}...";
            var commandLine = CurrentFeed.ApiKey is not null && CurrentFeed.ApiKey.Value.Length > 0
                ? $"nuget delete -Source {CurrentFeed.Uri.OriginalString} {package.Name} {package.Version} -ApiKey {CurrentFeed.ApiKey.Value} -NonInteractive"
                : $"nuget delete -Source {CurrentFeed.Uri.OriginalString} {package.Name} {package.Version} -NonInteractive";
            var command = new ShellCommand(commandLine);
            await command.ExecVoidAsync();
            Status += "OK";
        }
        await Task.Delay(1000);
        Status = null;
    }

    private bool IsInitialized;

}