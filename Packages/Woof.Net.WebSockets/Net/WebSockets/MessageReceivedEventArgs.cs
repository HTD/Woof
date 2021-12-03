namespace Woof.Net.WebSockets;

/// <summary>
/// Event data for the WebSocket MessageReceived events.
/// </summary>
public class MessageReceivedEventArgs : WebSocketEventArgs {

    /// <summary>
    /// Gets the decoded message.
    /// </summary>
    public object? Message => DecodeResult.Message;

    /// <summary>
    /// Gets the decoded message identifier.
    /// </summary>
    public Guid MessageId => DecodeResult.MessageId;

    /// <summary>
    /// Gets the additional message metadata.
    /// </summary>
    public DecodeResult DecodeResult { get; }

    /// <summary>
    /// Creates new WebSocket MessageReceived event data.
    /// </summary>
    /// <param name="decodeResult">Message decoding result.</param>
    /// <param name="context">WebSocket context.</param>
    public MessageReceivedEventArgs(DecodeResult decodeResult, WebSocketContext context)
        : base(context) => DecodeResult = decodeResult;

}
