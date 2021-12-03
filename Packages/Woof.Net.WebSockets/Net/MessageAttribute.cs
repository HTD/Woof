namespace Woof.Net;

/// <summary>
/// Assigns a type identifier to a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MessageAttribute : Attribute {

    /// <summary>
    /// Creates new message type attribute.
    /// </summary>
    /// <param name="id">Message type identifier.</param>
    public MessageAttribute(int id) => MessageTypeId = id;

    /// <summary>
    /// Gets the message type identifier.
    /// </summary>
    public int MessageTypeId { get; }

    /// <summary>
    /// Gets the value indicating whether the message is required to be signed.
    /// </summary>
    public bool IsSigned { get; set; }

    /// <summary>
    /// Gets the value indicating whether the message is a special sign-in message.
    /// </summary>
    public bool IsSignInRequest { get; set; }

    /// <summary>
    /// Gets a value indicating whether the message is an error message.
    /// </summary>
    public bool IsError { get; set; }

}
