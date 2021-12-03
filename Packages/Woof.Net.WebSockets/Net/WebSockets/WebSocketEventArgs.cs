namespace Woof.Net.WebSockets;

/// <summary>
/// Event data for WebSocket context events.
/// </summary>
public class WebSocketEventArgs : EventArgs {

    /// <summary>
    /// Gets the WebSocket context.
    /// </summary>
    public WebSocketContext Context { get; }

    /// <summary>
    /// Creates WebSocket context event data.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    public WebSocketEventArgs(WebSocketContext context) => Context = context;

}
