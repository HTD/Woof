
using Woof.Net.Messages;

namespace Woof.Net;

/// <summary>
/// Message type with some metadata.
/// </summary>
public class MessageTypeContext {

    /// <summary>
    /// Gets the message type identifier, if it was set in the constructor. Zero otherwise.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the message type.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Gets a value indicating whether the message should be signed.
    /// </summary>
    public bool IsSigned { get; }

    /// <summary>
    /// Gets a value indicating whether the message is a sign in request.
    /// </summary>
    public bool IsSignInRequest { get; }

    /// <summary>
    /// Gets a value indicating whether the message is an error message.
    /// </summary>
    public bool IsError { get; }

    /// <summary>
    /// Creates a new message type context.
    /// </summary>
    /// <param name="id">Message type identifier.</param>
    /// <param name="messageType">Message type.</param>
    /// <param name="isSigned">True if message should be signed.</param>
    /// <param name="isSignInRequest">True if the message is a sign in request.</param>
    /// <param name="isError">True is the message is an error message.</param>
    public MessageTypeContext(int id, Type messageType, bool isSigned = false, bool isSignInRequest = false, bool isError = false) {
        Id = id;
        MessageType = messageType;
        IsSigned = isSigned;
        IsSignInRequest = isSignInRequest || messageType.GetInterface(nameof(ISignInRequest)) != null;
    }

    /// <summary>
    /// Creates a new message type context.
    /// </summary>
    /// <param name="messageType">Message type.</param>
    /// <param name="isSigned">True if message should be signed.</param>
    /// <param name="isSignInRequest">True if the message is a sign in request.</param>
    /// <param name="isError">True is the message is an error message.</param>
    public MessageTypeContext(Type messageType, bool isSigned = false, bool isSignInRequest = false, bool isError = false) {
        MessageType = messageType;
        IsSigned = isSigned;
        IsSignInRequest = isSignInRequest || messageType.GetInterface(nameof(ISignInRequest)) != null;
    }

}
