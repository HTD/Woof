namespace Woof.Net;

/// <summary>
/// Implementation provides asynchronous Receive and Send methods for the transport layer.
/// </summary>
public interface IAsyncTransport {

    /// <summary>
    /// Gets a value indicating the transport is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Receives data from the transport to the memory buffer provided.
    /// </summary>
    /// <param name="buffer">Memory buffer.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task returning number of bytes read and whether the end of the message is reached.</returns>
    ValueTask<(int, bool)> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends data from memory buffer to the transport.
    /// </summary>
    /// <param name="buffer">Memory buffer.</param>
    /// <param name="endOfMessage">True if data sent is the end of the message.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task completed when the sending is done.</returns>
    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, bool endOfMessage, CancellationToken cancellationToken = default);

}
