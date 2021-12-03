namespace Woof.Net.WebSockets;

/// <summary>
/// Event data for WebSocket events containing <see cref="UserSession"/> data.
/// </summary>
public sealed class WebSocketSessionEventArgs : WebSocketEventArgs {

    /// <summary>
    /// Gets the <see cref="UserSession"/> data.
    /// </summary>
    public UserSession? Session { get; }

    /// <summary>
    /// Creates WebSocet event data.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    /// <param name="session"><see cref="UserSession"/> data.</param>
    public WebSocketSessionEventArgs(WebSocketContext context, UserSession? session = default) : base(context)
        => Session = session;

}