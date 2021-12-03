using System.Net.WebSockets;

namespace Woof.Net;

/// <summary>
/// Defines message decoding result.
/// </summary>
public class DecodeResult {

    /// <summary>
    /// Gets the decoded message.
    /// </summary>
    public object? Message { get; }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public Guid MessageId { get; }

    /// <summary>
    /// Gets the message type context.
    /// </summary>
    public MessageTypeContext? TypeContext { get; }

    /// <summary>
    /// Gets a value indicating whether the CLOSE frame was received instead of a message.
    /// </summary>
    public bool IsCloseFrame { get; }

    /// <summary>
    /// Gets a value indicating whether the message signature is valid.
    /// </summary>
    public bool IsSignatureValid { get; }

    /// <summary>
    /// Gets a value indicating whether the message is unauthorized:<br/>
    /// It's not a sign-in message, it should be signed, but the signature is not valid.
    /// </summary>
    public bool IsUnauthorized { get; }

    /// <summary>
    /// Gets a value indicating wheter the message was received correctly.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets an exception caught during receiving the message.
    /// </summary>
    public Exception? Exception { get; }

    ///// <summary>
    ///// Gets the WebSocket close status if the CLOSE frame was received.
    ///// </summary>
    //public WebSocketCloseStatus? CloseStatus { get; }

    ///// <summary>
    ///// Gets the WebSocket close status description if the CLOSE frame was received.
    ///// </summary>
    //public string? CloseStatusDescription { get; }

    /// <summary>
    /// Creates "full" decode result with the received and decoded message.
    /// </summary>
    /// <param name="typeContext">Message type context.</param>
    /// <param name="message">Message content.</param>
    /// <param name="id">Message identifier.</param>
    /// <param name="isValidSignatureRequired">True, if a valid signature of the message is required.</param>
    /// <param name="isSignatureValid">True, if the message signature is verified.</param>
    /// <param name="exception">Optional exception, if <see cref="IAuthenticationProvider"/> thrown it while decoding.</param>
    public DecodeResult(MessageTypeContext? typeContext, object? message, Guid id, bool isValidSignatureRequired = false, bool isSignatureValid = false, Exception? exception = null) {
        TypeContext = typeContext;
        MessageId = id;
        Message = message;
        IsSignatureValid = isSignatureValid;
        IsUnauthorized = isValidSignatureRequired && !isSignatureValid;
        IsSuccess = true;
        Exception = exception;
    }

    /// <summary>
    /// Creates "close" decode result from <see cref="WebSocketReceiveResult"/>.
    /// </summary>
    /// <param name="isCloseFrame">Set true to indicate that a close frame was received. False for empty frame.</param>
    public DecodeResult(bool isCloseFrame = false) {
        IsCloseFrame = isCloseFrame;
        IsSuccess = true;
    }

    /// <summary>
    /// Creates error decode result when the full message metadata is read.
    /// </summary>
    /// <param name="exception">Exception while receiving the message.</param>
    /// <param name="typeContext">Message type context.</param>
    /// <param name="id">Message identifier.</param>
    public DecodeResult(Exception exception, MessageTypeContext typeContext, Guid id) {
        TypeContext = typeContext;
        MessageId = id;
        Exception = exception;
    }

    /// <summary>
    /// Creates error decode result when the message metadata is read, but the type information is not available.
    /// </summary>
    /// <param name="exception">Exception while receiving the message.</param>
    /// <param name="id">Message identifier.</param>
    public DecodeResult(Exception exception, Guid id) {
        MessageId = id;
        Exception = exception;
    }

    /// <summary>
    /// Creates error decode result when the message metadata could not be read.
    /// </summary>
    /// <param name="exception"></param>
    public DecodeResult(Exception exception) => Exception = exception;

}
