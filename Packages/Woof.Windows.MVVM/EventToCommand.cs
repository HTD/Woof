namespace Woof.Windows.Mvvm;

/// <summary>
/// A proxy class with a shortcut to <see cref="Interaction.GetTriggers(DependencyObject)"/>.
/// It's purpose is to save <see cref="Microsoft.Xaml.Behaviors"/> namespace declaration.
/// </summary>
public static class Mvvm {

    /// <summary>
    /// Gets the <see cref="Microsoft.Xaml.Behaviors.TriggerCollection"/> containing the triggers associated with the specified object.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>A <see cref="Microsoft.Xaml.Behaviors.TriggerCollection"/> containing the triggers associated with the specified object.</returns>
    public static Microsoft.Xaml.Behaviors.TriggerCollection GetEvents(DependencyObject obj) => Interaction.GetTriggers(obj);

}

/// <summary>
/// A MVVM trigger that listens for a specified event on its source and fires when that event is fired.
/// </summary>
public class MvvmEvent : Microsoft.Xaml.Behaviors.EventTrigger {

    /// <summary>
    /// Command property definition.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(MvvmEvent), null);

    /// <summary>
    /// Gets or sets the command property.
    /// </summary>
    public ICommand Command {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Handles the PropertyChanged events.
    /// </summary>
    /// <param name="e">Event data.</param>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
        base.OnPropertyChanged(e);
        if (e.NewValue == e.OldValue) return;
        switch (e.Property.Name) {
            case nameof(Command):
                Actions.Add(new MvvmAction((string)GetValue(EventNameProperty), (ICommand)e.NewValue));
                break;
        }
    }

}

/// <summary>
/// A <see cref="Microsoft.Xaml.Behaviors.TriggerAction"/> that allows mapping events to MVVM commands.
/// </summary>
public sealed class MvvmAction : TriggerAction<DependencyObject> {

    /// <summary>
    /// Creates a binding action between the event name and the command.
    /// </summary>
    /// <param name="eventName">Name of the event.</param>
    /// <param name="command">Command to bind.</param>
    public MvvmAction(string eventName, ICommand command) {
        Command = command;
        EventName = eventName;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">Event arguments.</param>
    protected override void Invoke(object parameter) => Command.Execute(new MvvmEventData(EventName, AssociatedObject, parameter));

    private readonly string EventName;
    private readonly ICommand Command;

}

/// <summary>
/// Complete event data for <see cref="MvvmAction"/>.
/// </summary>
public sealed class MvvmEventData {

    /// <summary>
    /// Gets the event sender.
    /// </summary>
    public object Sender { get; }

    /// <summary>
    /// Gets the event arguments.
    /// </summary>
    public object Arguments { get; }

    /// <summary>
    /// Gets the optional trigger action tag.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Creates the <see cref="MvvmEventData"/>.
    /// </summary>
    /// <param name="eventName">Name of the event.</param>
    /// <param name="sender">The dependency object that triggered the event.</param>
    /// <param name="arguments">Event arguments.</param>
    public MvvmEventData(string eventName, object sender, object arguments) {
        EventName = eventName;
        Sender = sender;
        Arguments = arguments;
    }

}
