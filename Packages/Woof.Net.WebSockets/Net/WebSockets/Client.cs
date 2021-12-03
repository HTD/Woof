using System.Net.WebSockets;

using Woof.Net.Messages;

namespace Woof.Net.WebSockets;

/// <summary>
/// WebSocket client base providing simple WebSocket transport and handshake over any given subprotocol.
/// </summary>
public abstract class Client<TCodec> : WebSocketEndpoint<TCodec>, IDisposable, IAsyncDisposable where TCodec : SubProtocolCodec, new() {

    #region Public API

    /// <summary>
    /// Gets or sets a globally unique client identifier.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets or sets the keep alive interval value for the <see cref="ClientWebSocket"/>. Default 30 seconds.
    /// </summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Starts the client conection.
    /// </summary>
    /// <returns>Task completed when initialized and the receiving task is started.</returns>
    public async ValueTask StartAsync() {
        if (EndPointUri is null) throw new NullReferenceException($"{nameof(EndPointUri)} must be set before calling {nameof(StartAsync)}");
        if (Codec.IsLoadingTypesRequired && !Codec.IsMessageTypesLoaded)
            throw new InvalidOperationException("Message types for the codec are required but not loaded");
        if (CTS != null || Context != null) throw new InvalidOperationException("Client already started");
        State = ServiceState.Starting;
        OnStateChanged(State);
        CTS = new CancellationTokenSource();
        using var timeoutCTS = new CancellationTokenSource(Timeout);
        using var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(CTS.Token, timeoutCTS.Token);
        var clientWebSocket = new ClientWebSocket();
        clientWebSocket.Options.KeepAliveInterval = KeepAliveInterval;
        Context = new WebSocketContext(clientWebSocket);
        if (Codec.SubProtocol != null)
            clientWebSocket.Options.AddSubProtocol(Codec.SubProtocol);
        try {
            IsConnected = false;
            await clientWebSocket.ConnectAsync(EndPointUri, linkedCTS.Token);
            IsConnected = true;
            await StartReceiveAsync(Context, linkedCTS.Token);
        }
        catch {
            await StopAsync();
            if (timeoutCTS.IsCancellationRequested)
                throw new TimeoutException();
            else throw;
        }
    }

    /// <summary>
    /// Stops the client connection.
    /// </summary>
    /// <returns>Task completed when all client tasks are stopped and the connection is closed.</returns>
    public async ValueTask StopAsync() {
        if (State is ServiceState.Stopping or ServiceState.Stopped) return;
        await StopAsyncCore();
        Dispose(false);
        Context = null;
        State = ServiceState.Stopped;
        OnStateChanged(State);
    }

    /// <summary>
    /// Sends a message to the server context.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <param name="typeHint">Type hint.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <returns>Task completed when the sending is done.</returns>
    public async ValueTask SendMessageAsync(object message, Type? typeHint = null, Guid id = default) {
        if (Context != null) await SendMessageAsync(message, typeHint, Context, id);
    }

    /// <summary>
    /// Sends a message to the server context.
    /// </summary>
    /// <typeparam name="T">Message type.</typeparam>
    /// <param name="message">Message to send.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <returns>Task completed when the sending is done.</returns>
    public async ValueTask SendMessageAsync<T>(T message, Guid id = default) {
        if (Context != null) await SendMessageAsync(message, Context, id);
    }

    /// <summary>
    /// Sends a message to the server context and awaits until the response of the specified type is received.
    /// </summary>
    /// <param name="request">Request message.</param>
    /// <returns>Task returning the response message.</returns>
    public async ValueTask<object?> SendAndReceiveAsync(object request)
        => Context != null ? await SendAndReceiveAsync(request, Context, Timeout) : null;

    /// <summary>
    /// Sends a message to the server context and awaits until the response of the specified type is received.
    /// </summary>
    /// <typeparam name="TRequest">Request message type.</typeparam>
    /// <typeparam name="TResponse">Response message type.</typeparam>
    /// <param name="request">Request message.</param>
    /// <returns>Task returning the response message.</returns>
    public async ValueTask<TResponse> SendAndReceiveAsync<TRequest, TResponse>(TRequest request)
        => Context is null
            ? throw new NullReferenceException("There is no context for that request. The Client is not started.")
            : await SendAndReceiveAsync<TRequest, TResponse>(request, Context);

    /// <summary>
    /// Performs asynchronous signing with an API key and the secret.
    /// If the authentication passes, an authorized client session is started.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    /// <param name="apiKey">API key.</param>
    /// <param name="apiSecret">API secret.</param>
    /// <returns>Task returning true if key and the secret are valid.</returns>
    public async Task SignInAsync(Guid clientId, string apiKey, string apiSecret) {
        if (Codec.SessionProvider is null) throw new NullReferenceException($"{nameof(Codec.SessionProvider)} is not set");
        var apiKeyData = Codec.GetKey(apiKey);
        var apiSecretData = Codec.GetKey(apiSecret);
        var request = new SignInRequest { ApiKey = apiKeyData, ClientId = clientId };
        var session = Codec.SessionProvider.GetSession<ClientSession>();
        session.Key = apiSecretData;
        await SendAndReceiveAsync<SignInRequest, SignInResponse>(request);
    }

    /// <summary>
    /// Ends the authorized client session.
    /// </summary>
    /// <returns>Task completed when the sign out message is sent.</returns>
    public async Task SignOutAsync() => await SendMessageAsync(new SignOutRequest());

    /// <summary>
    /// Tests the connectivity. Return request-response round trip time.
    /// </summary>
    /// <returns>Request-response round trip time.</returns>
    public async ValueTask<TimeSpan> PingAsync() {
        var t0 = DateTime.Now;
        await SendAndReceiveAsync<PingRequest, PingResponse>(new PingRequest());
        return DateTime.Now - t0;
    }

    /// <summary>
    /// Gets the server identification data.
    /// </summary>
    /// <returns>Task completed when the data is fetched.</returns>
    public ValueTask<IdResponse> IdAsync() => SendAndReceiveAsync<IdRequest, IdResponse>(new());

    /// <summary>
    /// Queries the server for the available API messages.
    /// </summary>
    /// <param name="noInternals">Set true to exclude internal API messages.</param>
    /// <returns>Task returning the avaliable messages dictionary.</returns>
    public async ValueTask<Dictionary<int, string>> QueryApiAsync(bool noInternals = false)
        => (await SendAndReceiveAsync<QueryApiRequest, QueryApiResponse>(new() { NoInternals = noInternals })).Messages;

    /// <summary>
    /// Downloads a stream, or get its fragment.
    /// </summary>
    /// <param name="id">Requested stream identifier.</param>
    /// <param name="length">Requested length or zero, to load entire stream.</param>
    /// <param name="position">Stream position to load from, set -1 to use default / current position.</param>
    /// <returns><see cref="ValueTask"/> returning downloaded data as <see cref="ReadOnlyMemory{T}"/>.</returns>
    public async ValueTask<ReadOnlyMemory<byte>> DownloadAsync(Guid id, int length = default, long position = -1) {
        GetStreamFragmentResponse response;
        var bytesRequested = length > 0 ? length : 0x10000;
        using var buffer = new MemoryStream(bytesRequested);
        var isEnd = false;
        while (!isEnd) {
            response = await SendAndReceiveAsync<GetStreamFragmentRequest, GetStreamFragmentResponse>(
                new GetStreamFragmentRequest {
                    Id = id,
                    Length = length,
                    Position = position
                }
            );
            position = -1;
            buffer.Write(response.Buffer);
            isEnd = response.IsEnd;
        }
        return buffer.ToArray();
    }

    /// <summary>
    /// Disposes all resources used by the client.
    /// Closes the connection if not already closed.
    /// </summary>
    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Shutdown

    /// <summary>
    /// Stops the client if started or starting. The client stays in stopping state until it's disposed.
    /// </summary>
    /// <returns>A task completed when the client is stopped.</returns>
    private async ValueTask StopAsyncCore() {
        if (State is ServiceState.Stopping or ServiceState.Stopped) return;
        State = ServiceState.Stopping;
        OnStateChanged(State);
        try {
            if (IsConnected && Context is not null && CTS is not null) {
                CTS.CancelAfter(Timeout);
                await Context.Transport.CloseAsync(WebSocketCloseStatus.NormalClosure, default, CTS.Token);
            }
        }
        catch (Exception) { }
        finally {
            IsConnected = false;
            CTS!.Dispose();
            CTS = null;
        }
    }

    /// <summary>
    /// Stops the server if started and not already stopped. Disposes all disposables.
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore() {
        await StopAsyncCore();
        Dispose(true);
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Stops the client when the server is closed.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the client is closed.</returns>
    protected override ValueTask OnReceiveEndAsync(WebSocketContext context)
        => State != ServiceState.Stopping ? StopAsync() : ValueTask.CompletedTask;

    #endregion

    #region Data fields

    /// <summary>
    /// WebSocket context used to exchange binary data with the server.
    /// </summary>
    WebSocketContext? Context;

    bool IsConnected;

    #endregion

}
