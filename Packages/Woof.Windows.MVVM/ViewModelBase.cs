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
    /// Notifies the view the property has changed and it needs to update.
    /// </summary>
    /// <param name="propertyName"></param>
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Trigger to enable or disable a control bound to a command.
    /// </summary>
    protected virtual void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

}
