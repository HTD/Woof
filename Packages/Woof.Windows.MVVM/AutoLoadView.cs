namespace Woof.Windows.Mvvm;

/// <summary>
/// A user control that automatically loads the view when it first becomes visible
/// if its DataContext implements <see cref="IGetAsync"/> interface.
/// </summary>
public class AutoLoadView : UserControl {

    /// <summary>
    /// Handles PropertyChanged event to detect when the control is shown for the first time to load its view-model.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
        base.OnPropertyChanged(e);
        if (e.Property == IsVisibleProperty) {
            if (DataContext is IGetAsync viewModel
                && e.NewValue is bool value && value
                && !viewModel.IsLoaded) await viewModel.GetAsync();
        }
    }

}