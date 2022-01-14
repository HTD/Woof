namespace Publish.ViewModels;

public class MainView : ViewModelBase, IGetAsync {

    public Visibility SpinnerVisibility { get; set; } = Visibility.Hidden;

    public ObservableList<string> Targets { get; set; } = new();

    public ObservableList<PackageNode> Packages { get; set; } = new();

    public bool IsLoaded { get; private set; }

    public async ValueTask GetAsync() {
        await Settings.Default.LoadAsync();
        if (Settings.Default.Targets is null) return;
        Targets.AddRange(Settings.Default.Targets);
        await Packages.GetAsync();
        IsLoaded = true;
    }

    public override bool CanExecute(object? parameter) => true;

    private async ValueTask ReloadRepositoryAsync() {
        try {
            SpinnerShow();
            await NugetCli.ReloadRepositoryAsync();
            IsLoaded = false;
            Packages.Clear();
            await Packages.GetAsync();
            IsLoaded = true;

        }
        catch {
            MessageBox.Show("Reload failed", "RELOAD", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally {
            SpinnerHide();
        }
    }

    private async ValueTask PublishPackagesAsync(IEnumerable<PackageItem> packages) {
    }

    private async ValueTask DeletePackagesAsync(IEnumerable<PackageItem> packages) {
    }

    public override async void Execute(object? parameter) {
        if (parameter is not string command) return;
        switch (command) {
            case "Reload":
                await ReloadRepositoryAsync();
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
        }
    }

    private void SpinnerShow() {
        SpinnerVisibility = Visibility.Visible;
        OnPropertyChanged(nameof(SpinnerVisibility));
    }

    private void SpinnerHide() {
        SpinnerVisibility = Visibility.Hidden;
        OnPropertyChanged(nameof(SpinnerVisibility));
    }

}
