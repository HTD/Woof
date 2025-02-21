using System.Reflection;
using System.Security.Cryptography;

namespace Woof.Net;

/// <summary>
/// An interface of subprotocol codecs for WebSocket servers and clients.<br/>
/// Allows implementing virtually any subprotocol for a WebSocket API.
/// </summary>
public abstract class SubProtocolCodec {

    /// <summary>
    /// Gets the subprotocol name.
    /// </summary>
    public abstract string? SubProtocol { get; }

    /// <summary>
    /// Gets the buffer serializer implementation.
    /// </summary>
    protected abstract IBufferSerializer Serializer { get; }

    /// <summary>
    /// Provides session management for both client and server.
    /// </summary>
    public SessionProvider? SessionProvider { get; set; }

    /// <summary>
    /// Gets or sets a module that allows ansynchronous authentication of the API key.
    /// </summary>
    public IAuthenticationProvider? AuthenticationProvider { get; set; }

    /// <summary>
    /// Gets a value indicating that loading of message types is required.
    /// </summary>
    public virtual bool IsLoadingTypesRequired { get; }

    /// <summary>
    /// Gets a value indicating whether message types are loaded.
    /// </summary>
    public bool IsMessageTypesLoaded => MessageTypes.Any();

    /// <summary>
    /// Gets the new unique message identifier.
    /// </summary>
    public abstract Guid NewId { get; }

    /// <summary>
    /// Gets the message types available in the API.
    /// </summary>
    internal MessageTypeDictionary MessageTypes { get; } = [];

    /// <summary>
    /// In derived class loads message types from the current application domain assemblies.
    /// WARNING: to load types from an external assembly use <see cref="LoadMessageTypes{T}"/> override.
    /// </summary>
    public virtual void LoadMessageTypes() { }

    /// <summary>
    /// Loads message types from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">Any public type present in the target assembly.</typeparam>
    public void LoadMessageTypes<T>() {
        Assembly.GetAssembly(typeof(T));
        LoadMessageTypes();
    }

    /// <summary>
    /// Reads and decodes a message from the WebSocket context.
    /// </summary>
    /// <param name="transport">WebSocket context.</param>
    /// <param name="limit">Optional message length limit, applied if positive value provided.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task returning decoded message with the identifier.</returns>
    public abstract ValueTask<DecodeResult?> DecodeMessageAsync(IAsyncTransport transport, int limit = -1, CancellationToken token = default);

    /// <summary>
    /// Sends a binary (octet stream) message with the appropriate header.
    /// </summary>
    /// <param name="transport">WebSocket context.</param>
    /// <param name="typeContext">Type context.</param>
    /// <param name="buffer">A binary data buffer.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task completed when the message is sent.</returns>
    public abstract ValueTask SendBinaryAsync(IAsyncTransport transport, MessageTypeContext typeContext, ReadOnlyMemory<byte> buffer, Guid id = default, CancellationToken token = default);

    /// <summary>
    /// Encodes the message and sends it to the WebSocket context.
    /// </summary>
    /// <param name="transport">WebSocket context.</param>
    /// <param name="message">Message to send.</param>
    /// <param name="typeHint">Type hint.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task completed when the message is sent.</returns>
    public abstract ValueTask SendEncodedAsync(IAsyncTransport transport, object message, Type? typeHint = null, Guid id = default, CancellationToken token = default);

    /// <summary>
    /// Encodes the message and sends it to the WebSocket context.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <param name="transport">WebSocket context.</param>
    /// <param name="message">Message to send.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task completed when the message is sent.</returns>
    public abstract ValueTask SendEncodedAsync<TMessage>(IAsyncTransport transport, TMessage message, Guid id = default, CancellationToken token = default);

    /// <summary>
    /// Signs a serialized message payload with a type of HMAC algorithm.
    /// </summary>
    /// <param name="message">Binary message payload.</param>
    /// <param name="key">Message signing key.</param>
    /// <returns>Message signature (20 bytes, 160 bits).</returns>
    public virtual byte[] Sign(ReadOnlyMemory<byte> message, byte[] key) {
        using var hmac = new HMACSHA256(key);
        var result = new byte[hmac.HashSize >> 3];
        return !hmac.TryComputeHash(message.Span, result.AsSpan(), out _) ? throw new InvalidOperationException() : result;
    }

    /// <summary>
    /// Gets a hash of a key.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns>32 bytes (128 bits).</returns>
    public virtual byte[] GetHash(byte[] apiKey) => SHA256.HashData(apiKey);

    /// <summary>
    /// Gets a new key.
    /// </summary>
    /// <returns>64 bytes (256 bits).</returns>
    public virtual byte[] GetKey() {
        using var hmac = new HMACSHA256();
        return hmac is null ? throw new NullReferenceException() : hmac.Key;
    }

    /// <summary>
    /// Gets a key from string.
    /// </summary>
    /// <param name="keyString">Key string.</param>
    /// <returns>Key bytes.</returns>
    public virtual byte[] GetKey(string keyString) {
        try {
            return Convert.FromBase64String(keyString);
        }
        catch {
            throw new AuthenticationException(AuthenticationError.ApiAccessDenied);
        }
    }

    /// <summary>
    /// Gets a key string from key bytes.
    /// </summary>
    /// <param name="key">Key bytes.</param>
    /// <returns>Key string.</returns>
    public virtual string GetKeyString(byte[] key) => Convert.ToBase64String(key);

}
