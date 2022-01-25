namespace WoofRepositoryManager.ViewModels;

/// <summary>
/// Main view model.
/// </summary>
public class MainView : ViewModelBase, IGetAsync {

    /// <summary>
    /// Gets or sets the visibility value for the view loading spinner.
    /// </summary>
    public Visibility SpinnerVisibility { get; set; } = Visibility.Hidden;

    /// <summary>
    /// Gets the targets for the view.
    /// </summary>
    public ObservableList<Settings.NuGetFeed> Feeds { get; } = new();

    /// <summary>
    /// Gets the packages for the view.
    /// </summary>
    public ObservableList<PackageNode> Packages { get; } = new();

    /// <summary>
    /// Gets a value indicating the view data is loaded.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Gets the view data, here - initializes the application.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the view data is loaded.</returns>
    public async ValueTask GetAsync() {
        Executable.ResetAssembly<Settings>(); // a hack for the designer to work!
        await Settings.Default.LoadAsync();
        Feeds.AddRange(Settings.Default.Feeds);
        await Packages.GetAsync();
        IsLoaded = true;
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
            case "Reload":
                await CallAsync(command, NugetCli.ReloadRepositoryAsync, reloadView: true);
                break;
            case "Reset":
                await CallAsync(command, NugetCli.ResetRepository, reloadView: true);
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
                await PublishPackagesAsync(Packages.GetChecked());
                break;
            case "Delete":
                await DeletePackagesAsync(Packages.GetChecked());
                break;
            case "Settings":
                var settingsDirectory = UserFiles.UserDirectory;
                if (!settingsDirectory.Exists) settingsDirectory.Create();
                var showSettingsDirectoryCommand = new ShellCommand($"explorer \"{settingsDirectory}\"");
                await showSettingsDirectoryCommand.ExecAndForgetAsync();
                break;
        }
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
            SpinnerShow();
            await asyncAction();
            if (reloadView) {
                IsLoaded = false;
                Packages.Clear();
                await Packages.GetAsync();
                IsLoaded = true;
            }
        }
        catch {
            MessageBox.Show($"{commandName} failed", commandName, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally {
            SpinnerHide();
        }
    }

    /// <summary>
    /// Calls a synchronous action and displays a spinner until the action completes. Reloads the view optionally.
    /// </summary>
    /// <param name="commandName">A name of the command for an error message box if the action throws.</param>
    /// <param name="asyncAction">A synchronous action to call.</param>
    /// <param name="reloadView">True to reload the view after the action is completed.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the action completes and optionally the view is reloaded.</returns>
    private ValueTask CallAsync(string commandName, Action action, bool reloadView)
        => CallAsync(commandName, () => { action(); return ValueTask.CompletedTask; }, reloadView);

    /// <summary>
    /// Publishes the selected packages to the selected target feed.
    /// </summary>
    /// <param name="packages">Packages selected.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the packages are published.</returns>
    private async ValueTask PublishPackagesAsync(IEnumerable<PackageItem> packages) {
    }

    /// <summary>
    /// Deletes or unlists the selected packages from the selected target feed.
    /// </summary>
    /// <param name="packages">Packages selected.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the packages are deleted / unlisted.</returns>
    private async ValueTask DeletePackagesAsync(IEnumerable<PackageItem> packages) {
    }

    /// <summary>
    /// Shows the view loading spinner animation.
    /// </summary>
    private void SpinnerShow() {
        SpinnerVisibility = Visibility.Visible;
        OnPropertyChanged(nameof(SpinnerVisibility));
    }

    /// <summary>
    /// Hides the view loading spinner animation.
    /// </summary>
    private void SpinnerHide() {
        SpinnerVisibility = Visibility.Hidden;
        OnPropertyChanged(nameof(SpinnerVisibility));
    }

}