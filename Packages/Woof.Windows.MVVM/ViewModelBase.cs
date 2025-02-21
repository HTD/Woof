namespace Woof.Windows.Mvvm;

/// <summary>
/// Basic abstract <see cref="INotifyPropertyChanged"/> and <see cref="ICommand"/> implementation.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged, ICommand {

    /// <summary>
    /// Occurs when availability of commands for the view has changed.
    /// Views are notified with this event to make controls enabled or disabled.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Occurs when bound property is changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Override this to provide command availability to the view.
    /// </summary>
    /// <param name="parameter">Visual controls pass the parameter to test if a command for it can be executed by the view model.</param>
    /// <returns></returns>
    virtual public bool CanExecute(object? parameter) => true;

    /// <summary>
    /// Override this to provide action triggered by the view.
    /// </summary>
    /// <param name="parameter">Optional parameter passed from the visual control.</param>
    virtual public void Execute(object? parameter) { }

    /// <summary>
    /// Gets a bound property value.
    /// </summary>
    /// <typeparam name="T">Type of the bound property.</typeparam>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="defaultValue">Default value, if not set.</param>
    /// <returns>Property value.</returns>
    protected T? GetValue<T>(string propertyName, T? defaultValue = default)
        => Properties.TryGetValue(propertyName, out var propertyValue) ? (T?)propertyValue : defaultValue;

    /// <summary>
    /// Sets a bound property value.
    /// </summary>
    /// <typeparam name="T">Type of the bound property.</typeparam>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="defaultValue">Default value to test if the value changed.</param>
    protected void SetValue<T>(string propertyName, T? value, T? defaultValue = default) {
        var oldValue = GetValue(propertyName, defaultValue);
        if ((value is not null && !value.Equals(oldValue)) || oldValue is not null) {
            Properties[propertyName] = value;
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Notifies the view the property has changed and it needs to update.
    /// </summary>
    /// <param name="propertyName"></param>
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Trigger to enable or disable a control bound to a command.
    /// </summary>
    protected virtual void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Automatic bound properties dictionary to provide backing fields for the properties.
    /// </summary>
    private readonly Dictionary<string, object?> Properties = [];

}
