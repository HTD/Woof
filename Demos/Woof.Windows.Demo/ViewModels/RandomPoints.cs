using Woof.Windows.Demo.Models;

namespace Woof.Windows.Demo.ViewModels;

internal class RandomPoints : ViewModelBase, IGetAsync {

    public ObservableList<DataPointObservable> Items { get; } = new();

    public ObservableList<Check> Checks { get; } = new();

    public bool IsLoaded { get; private set; }

    private bool IsLoading;

    public RandomPoints() {
        Items.Filter = FilterPredicate;
        Checks.Add(new Check("Z > 0"));
        Checks.Add(new Check("X > 0", true));
        Checks.Add(new Check("Y > 0"));
        Checks.PropertyChanged += Checks_PropertyChanged;
        Items.PropertyChanged += Item_PropertyChanged;
    }

    private bool FilterPredicate(DataPoint item) {
        if (item is null) return true;
        bool x = Checks.Any(c => c.Value is "X > 0" && c.IsChecked);
        bool y = Checks.Any(c => c.Value is "Y > 0" && c.IsChecked);
        bool z = Checks.Any(c => c.Value is "Z > 0" && c.IsChecked);
        return (!x || item.X > 0) && (!y || item.Y > 0) && (!z || item.Z > 0);
    }

    private void Checks_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Items.Refresh();

    private void Clear() => Items.Clear();

    private async void AddSome() {
        var startId = Items.Count > 0 ? Items.Max(i => i.Id) + 1 : 1;
        await Items.AddRangeAsync(DataPoint.GetSomeAsync(8, startId).AsObservable());
    }

    private async void Reload() {
        IsLoading = true;
        OnCanExecuteChanged();
        Items.Clear();
        await Items.AddRangeAsync(DataPoint.GetSomeAsync(256).AsObservable());
        IsLoading = false;
        OnCanExecuteChanged();
    }

    private void DeleteFirst() {
        if (Items.Count > 0) Items.RemoveAt(0);
    }

    private void InsertNew() {
        DataPointObservable? item = new();
        Items.Insert(0, item);
    }

    private void Replace() {
        foreach (var item in Items.ToArray()) {
            var replacement = new DataPointObservable { Id = item.Id, X = 1, Y = 2, Z = 3 };
            var index = Items.IndexOf(item);
            //Items.RemoveAt(index);
            //Items.Add(replacement);
            Items.Replace(item, replacement);
            //OnPropertyChanged(nameof(Items));
        }
    }

    public override bool CanExecute(object? parameter) =>
        parameter is MvvmEventData eventData && eventData.EventName == "Loaded" ||
        parameter is string cmd &&
        cmd
            is nameof(Clear)
            or nameof(AddSome)
            or nameof(Reload)
            or nameof(DeleteFirst)
            or nameof(InsertNew)
            or nameof(Replace)
            && IsLoaded && !IsLoading;

    public override void Execute(object? parameter) {
        if (parameter is MvvmEventData eventData) {
            if (eventData.EventName == "Loaded") {
                Debug.Print("Loaded event received.");
            }
        }
        switch (parameter) {
            case nameof(Clear): Clear(); break;
            case nameof(AddSome): AddSome(); break;
            case nameof(Reload): Reload(); break;
            case nameof(DeleteFirst): DeleteFirst(); break;
            case nameof(InsertNew): InsertNew(); break;
            case nameof(Replace): Replace(); break;
        }
    }

    public async ValueTask GetAsync() {
        IsLoaded = false;
        OnCanExecuteChanged();
        Items.Clear();
        await Items.AddRangeAsync(DataPoint.GetSomeAsync(256).AsObservable());
        Checks.Sort();
        IsLoaded = true;
        OnCanExecuteChanged();
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsLoading) return;
        MessageBox.Show($"Data point change detected:\r\n{sender}", "Hey!", MessageBoxButton.OK, MessageBoxImage.Information);
    }

}