namespace Woof.Net;

/// <summary>
/// Event data for the exception events.
/// </summary>
public class ExceptionEventArgs : EventArgs {

    /// <summary>
    /// Gets the exception that triggered the event.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Creates new event data for exception events.
    /// Extracts the inner-most exception that is most probable cause of the problem.
    /// </summary>
    /// <param name="exception">Exception that triggered the event.</param>
    public ExceptionEventArgs(Exception exception) {
        while (exception.InnerException != null) exception = exception.InnerException;
        Exception = exception;
    }

}
