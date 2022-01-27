namespace Woof.Windows.Controls;

/// <summary>
/// A control that displays checkable items as dropdown menu.<br/>
/// Bind to an observable collection of <see cref="Check"/> items.
/// </summary>
public partial class Checks : UserControl {

    #region API

    /// <summary>
    /// Gets or sets the empty selection placeholder value.
    /// </summary>
    public object Empty {
        get => GetValue(EmptyProperty);
        set => SetValue(EmptyProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of the items for the control.<br/>
    /// The items should be of <see cref="Check"/> type.<br/>
    /// The collection should be observable, like <see cref="ObservableCollection{T}"/> or <see cref="ObservableList{T}"/>.<br/>
    /// It should at least implement <see cref="INotifyPropertyChanged"/> and <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    public IEnumerable ItemsSource {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Initializes the control.
    /// </summary>
    public Checks() => InitializeComponent();

    #endregion

    #region Dependency properties

    /// <summary>
    /// <see cref="DependencyProperty"/> definition for <see cref="Empty"/> property.
    /// </summary>
    public static readonly DependencyProperty EmptyProperty =
        DependencyProperty.Register(
            nameof(Empty),
            typeof(object),
            typeof(Checks),
            new PropertyMetadata(new PropertyChangedCallback(OnEmptyPropertyChanged))
        );

    /// <summary>
    /// <see cref="DependencyProperty"/> definition for <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(Checks),
            new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged))
        );

    private static void OnEmptyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is Checks control && control.ItemsSource is not null && e.NewValue != e.OldValue)
            control.MenuContent.Header = e.NewValue;
    }

    private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
        if (sender is Checks control)
            control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
    }

    private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= OnCollectionChanged;
        if (newValue is INotifyCollectionChanged newCollection) {
            newCollection.CollectionChanged += OnCollectionChanged;
            ItemsSourceOnPropertyChangedMethod = GetItemsSourceOnPropertyChanged();
            if (newValue.OfType<object>().Any()) {
                foreach (object? item in newValue) MenuItemAdd(item);
                UpdateHeader();
            }
        }
    }

    void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is null) return;
                foreach (object? item in e.NewItems) MenuItemAdd(item);
                UpdateHeader();
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is null) return;
                foreach (object? item in e.OldItems) MenuItemRemove(item);
                UpdateHeader();
                break;
            case NotifyCollectionChangedAction.Replace:
                throw new NotImplementedException("Replace action is not implemented by Checks control");
            case NotifyCollectionChangedAction.Move:
                throw new NotImplementedException("Move action is not implemented by Checks control");
            case NotifyCollectionChangedAction.Reset:
                if (e.OldItems is not null || e.NewItems is not null)
                    throw new NotImplementedException("Reset action is supported by Checks control only without parameters");
                foreach (MenuItem? menuItem in MenuContent.Items.OfType<MenuItem>()) menuItem.Checked -= Item_Checked;
                MenuContent.Items.Clear();
                UpdateHeader();
                var ise = ItemsSource.GetEnumerator();
                ise.Reset();
                if (!ise.MoveNext()) return;
                foreach (object? item in ItemsSource) MenuItemAdd(item);
                break;
        }
    }

    #endregion

    #region Helpers

    private void UpdateHeader() {
        string?[]? labels = ItemsSource
            .OfType<Check>()
            .Where(i => i.IsChecked)
            .Select(i => i.Value?.ToString())
            .ToArray();
        object? header = labels.Length < 1 ? Empty : String.Join(", ", labels);
        MenuContent.Header = header;
    }

    private void MenuItemAdd(object item) {
        if (item is ICheckableValue c) {
            if (MenuContent.Items.OfType<MenuItem>().Any(i => i.Header == c.Value)) return;
            MenuItem? newItem = new() {
                Header = c.Value,
                IsCheckable = true,
                IsChecked = c.IsChecked,
            };
            newItem.Checked += Item_Checked;
            newItem.Unchecked += Item_Unchecked;
            MenuContent.Items.Add(newItem);
        }
        else throw new InvalidOperationException("Checks items must be of Woof.Windows.Mvvm.Check type");
    }

    private void MenuItemRemove(object item) {
        if (item is Check c) {
            MenuItem? menuItem = MenuContent.Items.OfType<MenuItem>().FirstOrDefault(i => i.Header == c.Value);
            if (menuItem is not null) {
                menuItem.Checked -= Item_Checked;
                menuItem.Unchecked -= Item_Unchecked;
                MenuContent.Items.Remove(menuItem);
            }
        }
    }

    private void Item_Checked(object sender, RoutedEventArgs e) {
        object? sourceValue = (e.Source as MenuItem)!.Header;
        Check? sourceItem = ItemsSource.OfType<Check>().FirstOrDefault(i => i.Value == sourceValue);
        if (sourceItem is null) return;
        sourceItem.IsChecked = true;
        UpdateHeader();
        NotifySourceItemChanged(sourceItem);
    }

    private void Item_Unchecked(object sender, RoutedEventArgs e) {
        object? sourceValue = (e.Source as MenuItem)!.Header;
        Check? sourceItem = ItemsSource.OfType<Check>().FirstOrDefault(i => i.Value == sourceValue);
        if (sourceItem is null) return;
        sourceItem.IsChecked = false;
        UpdateHeader();
        NotifySourceItemChanged(sourceItem);
    }

    #endregion

    #region ItemsSource notifier

    private void NotifySourceItemChanged(object item) => ItemsSourceOnPropertyChanged(item, new PropertyChangedEventArgs("IsChecked"));

    private void ItemsSourceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => ItemsSourceOnPropertyChangedMethod?.Invoke(ItemsSource, new object?[] { sender, e });

    private MethodInfo? GetItemsSourceOnPropertyChanged() {
        if (ItemsSource is null) return null;
        foreach (MethodInfo? method in ItemsSource.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)) {
            ParameterInfo[]? parameters = method.GetParameters();
            if (parameters.Length == 2 &&
                parameters[0].ParameterType == typeof(object) &&
                parameters[1].ParameterType == typeof(PropertyChangedEventArgs)) return method;
        }
        return null;
    }

    private MethodInfo? ItemsSourceOnPropertyChangedMethod;

    #endregion

}