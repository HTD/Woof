using System.Net.WebSockets;

namespace Woof.Net.WebSockets;

/// <summary>
/// Implements <see cref="IAsyncTransport"/> interface over the <see cref="WebSocket"/> instance.
/// </summary>
public sealed class WebSocketTransport : IAsyncTransport, IDisposable {

    /// <summary>
    /// Gets the value indicating that the WebSocket transport is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets a value indicating the transport is connected.
    /// </summary>
    public bool IsConnected => Socket.State == WebSocketState.Open;

    /// <summary>
    /// Creates WebSocket <see cref="IAsyncTransport"/> implementation.
    /// </summary>
    /// <param name="socket">A connected and configured WebSocket.</param>
    /// <param name="messageType">Default message type for the transport.</param>
    public WebSocketTransport(WebSocket socket, WebSocketMessageType messageType = WebSocketMessageType.Binary) {
        Socket = socket;
        MessageType = messageType;
    }

    /// <summary>
    /// Receives data from the transport to the memory buffer provided.
    /// </summary>
    /// <param name="buffer">Memory buffer.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// Task returning number of bytes read and whether the end of the message is reached.
    /// Negative first value indicate the close message is received.
    /// </returns>
    public async ValueTask<(int, bool)> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        var result = await Socket.ReceiveAsync(buffer, cancellationToken);
        return result.MessageType == WebSocketMessageType.Close
            ? (-1, true)
            : result.MessageType != MessageType
                ? throw new InvalidOperationException($"Invalid message type received, {MessageType} expected")
                : (result.Count, result.EndOfMessage);
    }

    /// <summary>
    /// Sends data from memory buffer to the transport.
    /// </summary>
    /// <param name="buffer">Memory buffer.</param>
    /// <param name="endOfMessage">True if data sent is the end of the message.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task completed when the sending is done.</returns>
    public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, bool endOfMessage, CancellationToken cancellationToken = default) {
        await Semaphore.WaitAsync(cancellationToken); // sending data to the socket is limited to 1 thread only!
        try {
            if (!cancellationToken.IsCancellationRequested)
                await Socket.SendAsync(buffer, MessageType, endOfMessage, cancellationToken);
        }
        finally {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Disconnects the endpoint. If the handshake is initiated, it waits 
    /// </summary>
    /// <param name="reason">Indicates the reason for closing the WebSocket connection.</param>
    /// <param name="reasonDescription">Allows applications to specify a human readable explanation as to why the connection is closed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task completed when the closing handshake completes.</returns>
    public async ValueTask CloseAsync(WebSocketCloseStatus reason = WebSocketCloseStatus.NormalClosure, string? reasonDescription = default, CancellationToken cancellationToken = default) {
        await Semaphore.WaitAsync(cancellationToken);
        try {
            if (Socket.State == WebSocketState.CloseReceived)
                await Socket.CloseOutputAsync(reason, reasonDescription, cancellationToken);
            else if (!cancellationToken.IsCancellationRequested)
                await Socket.CloseAsync(reason, reasonDescription, cancellationToken);
        }
        finally {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Disposes the socket and the semaphore.
    /// </summary>
    public void Dispose() {
        if (IsDisposed) return;
        Socket.Dispose();
        Semaphore.Dispose();
        IsDisposed = true;
    }

    #region Fields

    /// <summary>
    /// WebSocket.
    /// </summary>
    public readonly WebSocket Socket;

    /// <summary>
    /// Default message type for messages sent.
    /// </summary>
    public readonly WebSocketMessageType MessageType;

    /// <summary>
    /// The <see cref="SemaphoreSlim"/> used to limit the access to the socket for the one thread.<br/>
    /// It should be awaited before each send operation and released after the operation completes.
    /// </summary>
    private readonly SemaphoreSlim Semaphore = new(1, 1);

    #endregion

}
