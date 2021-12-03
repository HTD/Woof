namespace Woof.Net;

/// <summary>
/// Data for service state changed events.
/// </summary>
public class StateChangedEventArgs : EventArgs {

    /// <summary>
    /// Gets the state of the client or server service.
    /// </summary>
    public ServiceState State { get; }

    /// <summary>
    /// Creates new data for the sevice state changed events.
    /// </summary>
    /// <param name="state"></param>
    public StateChangedEventArgs(ServiceState state) => State = state;

}
