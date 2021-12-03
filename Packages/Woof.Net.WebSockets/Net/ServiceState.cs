namespace Woof.Net;

/// <summary>
/// A basic state that a client or server class can have.
/// </summary>
public enum ServiceState {

    /// <summary>
    /// Stopped, not started, initial state.
    /// </summary>
    Stopped,
    /// <summary>
    /// During the start-up sequence process.
    /// </summary>
    Starting,
    /// <summary>
    /// Started, fully operational.
    /// </summary>
    Started,
    /// <summary>
    /// During the shut-down sequence process.
    /// </summary>
    Stopping

}
