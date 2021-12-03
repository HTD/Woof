namespace Woof.Internals;

/// <summary>
/// Implementing class can accept a message from another module and return a response.
/// </summary>
public interface IAcceptMessage {

    /// <summary>
    /// Accepts a message from another module and optionaly responds to it.
    /// </summary>
    /// <param name="message">A message object.</param>
    /// <returns>A response object.</returns>
    void Message(object message);

}