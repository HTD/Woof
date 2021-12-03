using System.Reflection;

using Woof.Net.Messages;

namespace Woof.Net;

/// <summary>
/// Implements WOOF subprotocol codec.
/// </summary>
public sealed class WoofCodec : SubProtocolCodec {

    #region Public API

    /// <summary>
    /// Common subprotocol name.
    /// </summary>
    public const string Name = "WOOF";

    /// <summary>
    /// Gets the Protocol Buffers serializer.
    /// </summary>
    protected override IBufferSerializer Serializer { get; } = new ProtoBufSerializer();

    /// <summary>
    /// Gets the subprotocol name.
    /// </summary>
    public override string SubProtocol => Name;

    /// <summary>
    /// Gets always true, because Woof subprotocol codec requires message types defined.
    /// </summary>
    public override bool IsLoadingTypesRequired => true;

    /// <summary>
    /// Gets the new unique message id.
    /// </summary>
    public override Guid NewId => Guid.NewGuid();

    /// <summary>
    /// Loads message types from the current application domain assemblies.
    /// WARNING: to load types from an external assembly use <see cref="SubProtocolCodec.LoadMessageTypes{T}"/>.
    /// </summary>
    public override void LoadMessageTypes() {
        foreach (var t in
            AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<MessageAttribute>() != null)
            .Select(t => new { Type = t, Meta = t.GetCustomAttribute<MessageAttribute>() })) {
            if (t.Meta is null) continue;
            var id = t.Meta.MessageTypeId;
            if (MessageTypes.ContainsKey(id)) throw new InvalidOperationException(
                $"Duplicate {nameof(t.Meta.MessageTypeId)} assigned to both {MessageTypes[id].MessageType.Name} and {t.Type.Name}");
            MessageTypes.Add(t.Meta.MessageTypeId, new MessageTypeContext(t.Meta.MessageTypeId, t.Type, t.Meta.IsSigned, t.Meta.IsSignInRequest, t.Meta.IsError));
        }
    }

    /// <summary>
    /// Reads and decodes a message from the socket.
    /// </summary>
    /// <param name="transport">Transport layer implementation.</param>
    /// <param name="limit">Optional message length limit, applied if positive value provided.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task returning decoded message with the identifier.</returns>
    public override async ValueTask<DecodeResult?> DecodeMessageAsync(IAsyncTransport transport, int limit = default, CancellationToken token = default) {
        var metaLengthBuffer = new Memory<byte>(new byte[1]);
        var (bytesRead, isEnd) = await transport.ReceiveAsync(metaLengthBuffer, token);
        if (bytesRead < 0)
            return new DecodeResult(isCloseFrame: true); // close message received.
        if (isEnd || bytesRead < 1)
            return new DecodeResult(new InvalidOperationException(EHeaderIncomplete));
        var metaLength = metaLengthBuffer.Span[0];
        var metaDataBuffer = new Memory<byte>(new byte[metaLength]);
        (bytesRead, isEnd) = await transport.ReceiveAsync(metaDataBuffer, token);
        if (bytesRead < 0) return new DecodeResult(isCloseFrame: true);
        if (bytesRead < metaLength)
            return new DecodeResult(new InvalidOperationException(EHeaderIncomplete));
        var metaData = Serializer.Deserialize<MessageMetadata>(metaDataBuffer);
        if (metaData is null) return new DecodeResult(new NullReferenceException(EMissingMetadata), default);
        if (!MessageTypes.ContainsKey(metaData.TypeId)) { // we do not know the type, but we know the length so we should read it anyway!
            var rawData = new Memory<byte>(new byte[metaData.PayloadLength]);
            await transport.ReceiveAsync(rawData, token);
            var exception = new InvalidOperationException(EUnknownType);
            exception.Data.Add(nameof(metaData.TypeId), metaData.TypeId);
            exception.Data.Add(nameof(rawData), rawData);
            return new DecodeResult(exception, metaData.Id);
        }
        if (limit >= 0 && metaData.PayloadLength > limit) {
            var exception = new InvalidOperationException(String.Format(ELengthExceeded, metaData.PayloadLength, limit));
            exception.Data[nameof(metaData.PayloadLength)] = metaData.PayloadLength;
            exception.Data[nameof(limit)] = limit;
            return new DecodeResult(exception, metaData.Id);
        }
        var typeContext = MessageTypes[metaData.TypeId];
        if (isEnd)
            return new DecodeResult(typeContext, Activator.CreateInstance(typeContext.MessageType), metaData.Id);
        var messageBuffer = new byte[metaData.PayloadLength];
        var currentOffset = 0;
        var currentLengthLeft = messageBuffer.Length;
        Memory<byte> messageSegment;
        do {
            messageSegment = new Memory<byte>(messageBuffer, currentOffset, currentLengthLeft);
            (bytesRead, isEnd) = await transport.ReceiveAsync(messageSegment, token);
            if (bytesRead < 0) return new DecodeResult(isCloseFrame: true);
            currentOffset += bytesRead;
            currentLengthLeft -= bytesRead;
        } while (!isEnd);
        messageSegment = new(messageBuffer, 0, currentOffset);
        if (messageSegment.Length < metaData.PayloadLength || !isEnd)
            return new DecodeResult(new InvalidOperationException(EMessageIncomplete), metaData.Id);
        var message = Serializer.Deserialize(MessageTypes[metaData.TypeId].MessageType, messageSegment);
        var isSignatureValid = false;
        var isSignInRequest = typeContext.IsSignInRequest || message is ISignInRequest;
        if (typeContext.IsSigned && metaData.Signature != null) {
            try {
                var key =
                    isSignInRequest
                    ? (message is ISignInRequest signInRequest && AuthenticationProvider != null
                        ? await AuthenticationProvider.GetKeyAsync(signInRequest.ApiKey)
                        : null
                    )
                    : SessionProvider?.GetKey(transport);
                if (key != null) {
                    var expected = Sign(messageSegment, key);
                    isSignatureValid = metaData.Signature.SequenceEqual(expected);
                }
            }
            catch (Exception exception) {
                // A result must be returned when IAuthenticationProvider throws to allow proper handling in implementing code:
                return new DecodeResult(typeContext, message, metaData.Id, !isSignInRequest && typeContext.IsSigned, isSignatureValid: false, exception);
            }
        }
        return new DecodeResult(typeContext, message, metaData.Id, !isSignInRequest && typeContext.IsSigned, isSignatureValid);
    }

    /// <summary>
    /// Encodes the message and sends it to the WebSocket context.
    /// </summary>
    /// <param name="transport">Transport layer implementation.</param>
    /// <param name="message">Message to send.</param>
    /// <param name="typeHint">Type hint.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task completed when the message is sent.</returns>
    public override async ValueTask SendEncodedAsync(IAsyncTransport transport, object message, Type? typeHint = null, Guid id = default, CancellationToken token = default) {
        if (!transport.IsConnected) return;
        var typeContext = MessageTypes.GetContext(message, typeHint);
        var messageBuffer = Serializer.Serialize(message, typeHint);
        await SendBinaryAsync(transport, typeContext, messageBuffer, id, token);
    }

    /// <summary>
    /// Encodes the message and sends it to the WebSocket context.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <param name="transport">Transport layer implementation.</param>
    /// <param name="message">Message to send.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task completed when the message is sent.</returns>
    public override async ValueTask SendEncodedAsync<TMessage>(IAsyncTransport transport, TMessage message, Guid id = default, CancellationToken token = default) {
        if (!transport.IsConnected) return;
        var typeContext = MessageTypes.GetContext<TMessage>();
        var messageBuffer = Serializer.Serialize(message);
        await SendBinaryAsync(transport, typeContext, messageBuffer, id, token);
    }

    /// <summary>
    /// Sends a binary (octet stream) message with the appropriate header.
    /// </summary>
    /// <param name="transport">Transport layer implementation.</param>
    /// <param name="typeContext">Type context.</param>
    /// <param name="buffer">A binary data buffer.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task completed when the message is sent.</returns>
    public override async ValueTask SendBinaryAsync(IAsyncTransport transport, MessageTypeContext typeContext, ReadOnlyMemory<byte> buffer, Guid id = default, CancellationToken token = default) {
        var isPayloadPresent = buffer.Length > 0;
        var key = typeContext.IsSigned ? SessionProvider?.GetKey(transport) : null;
        var metadata =
            key is null || !isPayloadPresent
            ? new MessageMetadata {
                Id = id == default ? NewId : id,
                TypeId = typeContext.Id,
                PayloadLength = buffer.Length
            }
            : new MessageMetadata {
                Id = id == default ? NewId : id,
                TypeId = typeContext.Id,
                PayloadLength = buffer.Length,
                Signature = Sign(buffer, key)
            };
        var metaDataBuffer = Serializer.Serialize(metadata);
        var metaSizeBuffer = new Memory<byte>(new byte[1]);
        if (metaDataBuffer.Length > 255) throw new InvalidOperationException();
        metaSizeBuffer.Span[0] = (byte)metaDataBuffer.Length;
        if (!transport.IsConnected) return;
        await transport.SendAsync(metaSizeBuffer, endOfMessage: false, token);
        if (!transport.IsConnected) return;
        await transport.SendAsync(metaDataBuffer, endOfMessage: !isPayloadPresent, token);
        if (!transport.IsConnected) return;
        if (isPayloadPresent) await transport.SendAsync(buffer, endOfMessage: true, token);
    }

    #endregion

    #region Exception messages

    private const string EHeaderIncomplete = "Header incomplete";
    private const string EMissingMetadata = "Missing message metadata";
    private const string EUnknownType = "Uknown message type";
    private const string ELengthExceeded = "Message length (0x{0:x8}) exceeds 0x{1:x8} limit";
    private const string EMessageIncomplete = "Message data incomplete";

    #endregion

}