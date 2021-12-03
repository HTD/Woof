namespace Woof.Net;

/// <summary>
/// Message type dictionary used to resolve message type context from type identifier read from message header.
/// </summary>
public class MessageTypeDictionary : Dictionary<int, MessageTypeContext> {

    /// <summary>
    /// Gets the type context for the specified instance if defined.
    /// </summary>
    /// <param name="instance">Message instance.</param>
    /// <param name="typeHint">Type hint.</param>
    /// <returns>Type context.</returns>
    public MessageTypeContext GetContext(object? instance, Type? typeHint = null) {
        if (instance is null && typeHint is null) throw new ArgumentNullException(nameof(typeHint));
        var type = typeHint ?? instance?.GetType();
        return Values.First(i => i.MessageType == type);
    }

    /// <summary>
    /// Gets the type context for the specified type if defined.
    /// </summary>
    /// <returns>Type context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the type does not exists in the dictionary.</exception>
    public MessageTypeContext GetContext<TMessage>() => Values.First(i => i.MessageType == typeof(TMessage));

}
