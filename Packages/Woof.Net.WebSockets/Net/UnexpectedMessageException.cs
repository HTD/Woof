namespace Woof.Net;

/// <summary>
/// A type of exception that is thrown when a defined, but unexpected message type is received.<br/>
/// </summary>
public class UnexpectedMessageException : Exception {

    /// <summary>
    /// Creates the exception from the message received.
    /// </summary>
    /// <param name="message">Decoded message received.</param>
    public UnexpectedMessageException(object? message) : base($"{message?.GetType()?.Name ?? "null"} received") => Message = message;

    /// <summary>
    /// Gets the decoded message received instead of the expected message.
    /// </summary>
    public new object? Message { get; set; }

}