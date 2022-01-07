using System.Net;
using System.Net.WebSockets;

namespace Woof.Net.WebSockets;

/// <summary>
/// Thread-safe, extended WebSocket context.
/// </summary>
public sealed class WebSocketContext : IDisposable {

    /// <summary>
    /// Gets a value indicating whether the underlying <see cref="WebSocket"/> is open.
    /// </summary>
    public bool IsOpen => Transport.Socket.State == WebSocketState.Open;

    /// <summary>
    /// Gets the <see cref="HttpListenerWebSocketContext"/> if the socket was obtained from <see cref="HttpListener"/>.
    /// </summary>
    public HttpListenerWebSocketContext? HttpContext { get; }

    /// <summary>
    /// Gets the server IP address and port number to which the request is directed.
    /// </summary>
    public IPEndPoint? LocalEndPoint { get; }

    /// <summary>
    /// Gets the client IP address and port number from which the request originated.
    /// </summary>
    public IPEndPoint? RemoteEndPoint { get; }

    /// <summary>
    /// Allows the remote endpoint to describe the reason why the connection was closed.
    /// </summary>
    public string? CloseStatusDescription => Transport.Socket.CloseStatusDescription;

    /// <summary>
    /// Indicates the reason why the remote endpoint initiated the close handshake.
    /// </summary>
    public WebSocketCloseStatus? CloseStatus => Transport.Socket.CloseStatus;

    /// <summary>
    /// Returns the current state of the WebSocket connection.
    /// </summary>
    public WebSocketState State => Transport.Socket.State;

    /// <summary>
    /// The subprotocol that was negotiated during the opening handshake.
    /// </summary>
    public string? SubProtocol => Transport.Socket.SubProtocol;

    /// <summary>
    /// Gets the context transport.
    /// </summary>
    public WebSocketTransport Transport { get; }

    /// <summary>
    /// Cretes the context from the <see cref="WebSocket"/>.
    /// </summary>
    /// <param name="socket">Base socket.</param>
    public WebSocketContext(WebSocket socket) => Transport = new(socket);

    /// <summary>
    /// Creates the context from the <see cref="HttpListenerWebSocketContext"/>.
    /// </summary>
    /// <param name="listenerContext">HTTP listener WebSocket context.</param>
    /// <param name="listenerRequest">HTTP request that started WebSocket connection.</param>
    public WebSocketContext(HttpListenerWebSocketContext listenerContext, HttpListenerRequest listenerRequest) {
        LocalEndPoint = listenerRequest.LocalEndPoint;
        RemoteEndPoint = listenerRequest.RemoteEndPoint;
        var xForwardedForHeader = listenerContext.Headers["X-Forwarded-For"];
        if (xForwardedForHeader != null)
            LocalEndPoint = IPEndPoint.Parse(xForwardedForHeader);
        if (LocalEndPoint.Port < 1) LocalEndPoint.Port = 443;
        HttpContext = listenerContext;
        Transport = new(HttpContext.WebSocket);
    }

    /// <summary>
    /// Disposes the semaphore and the socket.
    /// </summary>
    public void Dispose() => Transport.Dispose();

}
