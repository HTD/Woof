using System.Net.WebSockets;
using System.Net;
using Woof.Net.Messages;

namespace Woof.Net.WebSockets;

/// <summary>
/// WebSocket server base providing simple WebSocket transport and handshake over any given subprotocol.
/// </summary>
public abstract partial class Server<TCodec> : WebSocketEndpoint<TCodec>, IDisposable, IAsyncDisposable where TCodec : SubProtocolCodec, new() {

    #region Public API

    #region Events

    /// <summary>
    /// Occurs when a client is connected to the server.
    /// </summary>
    public event AsyncEventHandler<WebSocketEventArgs>? ClientConnected;

    /// <summary>
    /// Occurs when a client is disconnecting from the server it's the last time the client data is available.<br/>
    /// The connection is not closed until all handlers complete.
    /// </summary>
    public event AsyncEventHandler<WebSocketEventArgs>? ClientDisconnecting;

    /// <summary>
    /// Occurs when the connection with the client is closed.
    /// </summary>
    public event EventHandlerSlim<Guid?>? ClientDisconnected;

    /// <summary>
    /// Occurs when an exception is thrown during client connection process.
    /// </summary>
    public event AsyncEventHandler<ExceptionEventArgs>? ConnectException;

    /// <summary>
    /// Occurs when the <see cref="IAuthenticationProvider"/> granted client's access.
    /// </summary>
    public event AsyncEventHandler<WebSocketSessionEventArgs>? UserSignedIn;

    /// <summary>
    /// Occurs when the <see cref="IAuthenticationProvider"/> denied client's access.
    /// </summary>
    public event AsyncEventHandler<WebSocketSessionEventArgs>? UserSignInFail;

    /// <summary>
    /// Occurs when the user initiatied signing out.
    /// </summary>
    public event AsyncEventHandler<WebSocketSessionEventArgs>? UserSigningOut;

    /// <summary>
    /// Occurs when the user is signed out.
    /// </summary>
    public event EventHandlerSlim<Guid?>? UserSignedOut;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating that if client authentication fails it should be disconnected.
    /// </summary>
    public bool DisconnectOnAuthenticationFailed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that if client should be disconnected after signing out.
    /// </summary>
    public bool DisconnectOnSignOut { get; set; }

    #endregion

    #region Transport methods

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>Task completed when the server is initialized and in listening state.</returns>
    public Task StartAsync() {
        if (EndPointUri is null) throw new NullReferenceException($"{nameof(EndPointUri)} must be set before calling {nameof(StartAsync)}");
        if (Codec.IsLoadingTypesRequired && !Codec.IsMessageTypesLoaded)
            throw new InvalidOperationException("Message types for the codec are required but not loaded");
        if (CTS != null || Listener != null || State == ServiceState.Started) throw new InvalidOperationException("Server already started");
        if (State == ServiceState.Starting) throw new InvalidOperationException("Server is starting");
        State = ServiceState.Starting;
        OnStateChanged(State);
        CTS = new CancellationTokenSource();
        Listener = new HttpListener();
        var prefix = RxWS.Replace(EndPointUri.ToString(), "http");
        Listener.Prefixes.Add(prefix);
        Listener.Start();
        State = ServiceState.Started;
        TimeStarted = DateTime.Now;
        OnStateChanged(State);
        return AsyncLoop.FromIterationAsync(StartClientConnectionAsync, OnConnectExceptionAsync, null, false, CTS.Token);
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <returns>Task completed when all server tasks are stopped and all connections are closed.</returns>
    public async ValueTask StopAsync() {
        if (State is ServiceState.Stopping or ServiceState.Stopped) return;
        await StopAsyncCore();
        Dispose(false);
        Listener = null;
        State = ServiceState.Stopped;
        OnStateChanged(State);
    }

    /// <summary>
    /// Gets the session for the client identifier if the client is connected and signed in.
    /// </summary>
    /// <param name="id">Client identifier.</param>
    /// <returns>Client's session.</returns>
    public UserSession? GetClientSession(Guid id)
        => Codec.SessionProvider?.Sessions?.Values.OfType<UserSession>().FirstOrDefault(s => s.Client?.Id == id);

    /// <summary>
    /// Gets the connection context for the session.
    /// </summary>
    /// <param name="session">Client's session.</param>
    /// <returns>Connection context.</returns>
    public WebSocketContext? GetClientContext(ISession? session) {
        if (session is null || Codec.SessionProvider is not SessionProvider sessionProvider) return null;
        foreach (var context in Clients) {
            var contextSession = sessionProvider.GetExistingSession(context.Transport);
            if (contextSession is null) return null;
            if (contextSession == session) return context;
        }
        return null;
    }

    /// <summary>
    /// Gets the connection context for the client identifier.
    /// </summary>
    /// <param name="id">Client identifier.</param>
    /// <returns>Connection context.</returns>
    public WebSocketContext? GetClientContext(Guid id) => GetClientContext(GetClientSession(id));

    /// <summary>
    /// Sends a message to the specified context.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <param name="typeHint">Type hint.</param>
    /// <param name="context">Target context.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <returns>Task completed when the sending is done.</returns>
    public new ValueTask SendMessageAsync(object message, Type? typeHint, WebSocketContext context, Guid id = default)
        => base.SendMessageAsync(message, typeHint, context, id);

    /// <summary>
    /// Sends a message to the specified context.
    /// </summary>
    /// <typeparam name="T">Type of the message.</typeparam>
    /// <param name="message">Message to send.</param>
    /// <param name="context">Target context.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <returns>Task completed when the sending is done.</returns>
    public new ValueTask SendMessageAsync<T>(T message, WebSocketContext context, Guid id = default)
        => base.SendMessageAsync(message, context, id);

    /// <summary>
    /// Sends a message to the specified context and awaits until the response of the specified type is received.
    /// </summary>
    /// <param name="request">Request message.</param>
    /// <param name="context">Target context.</param>
    /// <returns>Task returning the response message.</returns>
    public ValueTask<object?> SendAndReceiveAsync(object request, WebSocketContext context)
        => base.SendAndReceiveAsync(request, context);

    /// <summary>
    /// Sends a message to the specified context and awaits until the response of the specified type is received.
    /// </summary>
    /// <typeparam name="TRequest">Request message type.</typeparam>
    /// <typeparam name="TResponse">Response message type.</typeparam>
    /// <param name="request">Request message.</param>
    /// <param name="context">Target context.</param>
    /// <returns>Task returning the response message.</returns>
    public ValueTask<TResponse> SendAndReceiveAsync<TRequest, TResponse>(TRequest request, WebSocketContext context)
        => base.SendAndReceiveAsync<TRequest, TResponse>(request, context);

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <param name="typeHint">Type hint.</param>
    public void BroadcastMessageAsync(object message, Type? typeHint = null)
        => Parallel.ForEach(Clients, async context => await base.SendMessageAsync(message, typeHint, context));

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <typeparam name="T">Type of the message.</typeparam>
    /// <param name="message">Message to send.</param>
    public void BroadcastMessageAsync<T>(T message)
        => Parallel.ForEach(Clients, async context => await base.SendMessageAsync(message, context));

    /// <summary>
    /// Disposes all resources used by the server.
    /// Closes all connections if not closed already.
    /// </summary>
    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    #endregion

    #endregion

    #region Shutdown

    /// <summary>
    /// Stops the server if started or starting. The server stays in stopping state until it's disposed.
    /// </summary>
    /// <returns>A task completed when the server is stopped.</returns>
    private async ValueTask StopAsyncCore() {
        if (CTS is null || Listener is null || State == ServiceState.Stopping) return;
        State = ServiceState.Stopping;
        OnStateChanged(State);
        CTS.CancelAfter(Timeout);
        while (Clients.Any()) {
            var client = Clients.Last();
            var statusDescription = "SERVER SHUTDOWN";
            await client.Transport.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription, default);
            client.Dispose();
            Clients.Remove(client);
        }
        Listener.Stop();
        CTS.Cancel();
        CTS.Dispose();
        CTS = null;
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

    #region Internal messages handling

    #region Authentication

    /// <summary>
    /// Handles the <see cref="SignInRequest"/> message.
    /// </summary>
    /// <param name="decodeResult">Message decoding result.</param>
    /// <param name="signInRequest">Request received.</param>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the message handling completed.</returns>
    private async ValueTask SignInAsync(DecodeResult decodeResult, SignInRequest signInRequest, WebSocketContext context) {
        if (Codec.AuthenticationProvider is null) throw new NullReferenceException($"{nameof(Codec.AuthenticationProvider)} is not set");
        if (Codec.SessionProvider is null) throw new NullReferenceException($"{nameof(Codec.SessionProvider)} is not set");
        var session = Codec.SessionProvider.GetSession<UserSession>(context.Transport);
        if (session is not null && session.User is not null) await SignOutAsync(context);
        if (!decodeResult.IsSignatureValid) {
            await failAsync(AuthenticationError.ApiAccessDenied);
            return;
        }
        session = Codec.SessionProvider.GetSession<UserSession>(context.Transport);
        session.User = await Codec.AuthenticationProvider.GetUserAsync(signInRequest.ApiKey);
        if (session.User is null) {
            await failAsync(AuthenticationError.UserAccessDenied);
            return;
        }
        try {
            session.Key = await Codec.AuthenticationProvider.GetKeyAsync(signInRequest.ApiKey);
        }
        catch {
            await failAsync(AuthenticationError.ApiAccessDenied);
            return;
        }
        if (session.Key is null) {
            await failAsync(AuthenticationError.ApiAccessDenied);
            return;
        }
        session.Client = await Codec.AuthenticationProvider.GetClientAsync(signInRequest.ClientId);
        if (session.Client is null || !decodeResult.IsSignatureValid) {
            await failAsync(AuthenticationError.ClientAccessDenied);
            return;
        }
        await SendMessageAsync(new SignInResponse(), context, decodeResult.MessageId);
        await OnUserSignedInAsync(new WebSocketSessionEventArgs(context, session));
        async ValueTask failAsync(AuthenticationError error) {
            if (session != null) {
                session.User = null;
                session.Key = null;
            }
            var response = new AuthenticationErrorResponse { Code = (int)error, Description = error.GetDescription() };
            await SendMessageAsync(response, context, decodeResult.MessageId);
            await OnUserSignInFailAsync(new WebSocketSessionEventArgs(context, session));
            if (DisconnectOnAuthenticationFailed)
                await context.Transport.CloseAsync(
                WebSocketCloseStatus.PolicyViolation,
                "Client authorization failed",
                CancellationToken
            );
        }
    }

    /// <summary>
    /// Handles the <see cref="SignOutRequest"/> message.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the session is closed.</returns>
    private async ValueTask SignOutAsync(WebSocketContext context) {
        var session = Codec.SessionProvider?.GetSession<UserSession>(context.Transport);
        var userId = session?.User?.Id;
        await OnUserSigningOutAsync(new WebSocketSessionEventArgs(context, session));
        if (session is not null) {
            session.User = null;
            session.Key = null;
        }
        if (DisconnectOnSignOut) {
            Codec.SessionProvider?.CloseSession(context.Transport);
            await context.Transport.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client signed out", CancellationToken);
        }
        OnUserSignedOut(userId);
    }

    #endregion

    #region Streaming support

    /// <summary>
    /// The derived class gets the stream based on identifier given.
    /// </summary>
    /// <param name="id">Stream identifier.</param>
    /// <returns>Requested stream.</returns>
    protected virtual Stream GetStreamById(Guid id) => throw new NotImplementedException();

    /// <summary>
    /// Handles <see cref="GetStreamFragmentRequest"/>. Reads the stream fragment and sends it to the client.
    /// </summary>
    /// <param name="decodeResult">Message decoding result.</param>
    /// <param name="getStreamFragmentRequest">Request.</param>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the message is handled.</returns>
    private async ValueTask GetStreamFragmentAsync(DecodeResult decodeResult, GetStreamFragmentRequest getStreamFragmentRequest, WebSocketContext context) {
        var stream = GetStreamById(getStreamFragmentRequest.Id);
        var position = getStreamFragmentRequest.Position;
        var length = getStreamFragmentRequest.Length;
        if (position >= 0) stream.Position = position;
        var bytesRequested = length;
        if (bytesRequested < 1 && stream.Length < int.MaxValue) bytesRequested = (int)stream.Length;
        var streamBuffer = new byte[bytesRequested];
        var endPosition = length > 0 ? stream.Position + length : stream.Length;
        var bytesRead = await stream.ReadAsync(streamBuffer.AsMemory(0, bytesRequested));
        var isEnd = stream.Position == endPosition;
        await SendMessageAsync(new GetStreamFragmentResponse { IsEnd = isEnd, Buffer = streamBuffer.AsSpan(0, bytesRead).ToArray() }, context, decodeResult.MessageId);
    }

    #endregion

    #region Identification API

    /// <summary>
    /// Sends the response to the ping request.
    /// </summary>
    /// <param name="decodeResult">Message decoding result.</param>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the response is sent.</returns>
    private ValueTask PingAsync(DecodeResult decodeResult, WebSocketContext context) => SendMessageAsync(new PingResponse(), context, decodeResult.MessageId);

    /// <summary>
    /// Sends the response to the end point identification request.
    /// </summary>
    /// <param name="decodeResult">Message decoding result.</param>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the response is sent.</returns>
    private async ValueTask IdAsync(DecodeResult decodeResult, WebSocketContext context) {
        var assembly = GetType().Assembly;
        var assemblyName = assembly.GetName();
        var id = new IdResponse {
            Name = assemblyName?.Name ?? "N/A",
            Version = assemblyName?.Version?.ToString() ?? "N/A",
            BuildTime = File.GetCreationTime(assembly.Location),
            Timeout = Timeout,
            UpTime = DateTime.Now > TimeStarted ? DateTime.Now - TimeStarted : TimeSpan.Zero
        };
        await SendMessageAsync(id, context, decodeResult.MessageId);
    }

    /// <summary>
    /// Sends the response to the end point query API request.
    /// </summary>
    /// <returns>Task completed when the response is sent.</returns>
    private async ValueTask QueryApiAsync(DecodeResult decodeResult, QueryApiRequest request, WebSocketContext context) {
        var session = Codec.SessionProvider?.GetSession<UserSession>(context.Transport);
        var isAuthenticated = session?.Key is not null;
        var messages = Codec.MessageTypes
            .Where(m =>
                (!request.NoInternals || m.Key >= 0x1000 && m.Key < 0x1000_0000) &&
                (!m.Value.IsSigned || isAuthenticated))
            .OrderBy(m => m.Key)
            .ToDictionary(k => k.Key, v => v.Value.MessageType.Name);
        var data = new QueryApiResponse {
            Messages = messages
        };
        await SendMessageAsync(data, context, decodeResult.MessageId);
    }

    #endregion

    #endregion

    #region Overrides

    /// <summary>
    /// Handles built in messages, like basic authentication, identification and streaming.<br/>
    /// Messages handled internally won't trigger <see cref="WebSocketEndpoint{TCodec}.MessageReceived"/> event.
    /// </summary>
    /// <param name="decodeResult"></param>
    /// <param name="context"></param>
    protected async override ValueTask OnMessageReceivedAsync(DecodeResult decodeResult, WebSocketContext context) {
        switch (decodeResult.Message) {
            case SignInRequest signInRequest:
                await SignInAsync(decodeResult, signInRequest, context);
                break;
            case SignOutRequest:
                await SignOutAsync(context);
                break;
            case PingRequest:
                await PingAsync(decodeResult, context);
                break;
            case IdRequest:
                await IdAsync(decodeResult, context);
                break;
            case QueryApiRequest queryApiRequest:
                await QueryApiAsync(decodeResult, queryApiRequest, context);
                break;
            case GetStreamFragmentRequest getStreamFragmentRequest:
                await GetStreamFragmentAsync(decodeResult, getStreamFragmentRequest, context);
                break;
            default:
                await base.OnMessageReceivedAsync(decodeResult, context);
                break;
        }

    }

    /// <summary>
    /// Called when a client is disconnected. Closes, disposes and removes the disconnected client's socket.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed when the socket is closed, disposed and removed.</returns>
    protected async override ValueTask OnReceiveEndAsync(WebSocketContext context) {
        if (Listener is null) return;
        var session = Codec.SessionProvider?.GetSession<UserSession>(context.Transport);
        var clientId = session?.Client?.Id;
        await OnClientDisconnectingAsync(new WebSocketEventArgs(context));
        Codec.SessionProvider?.CloseSession(context.Transport);
        if (CTS != null)
            await context.Transport.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CTS.Token);
        context.Dispose();
        Clients.Remove(context);
        OnClientDisconnected(clientId);
    }

    #endregion

    #region Event handlers

    /// <summary>
    /// Called when a client is connected.
    /// </summary>
    /// <returns>Task completed when receiving loop is started.</returns>
    private async Task StartClientConnectionAsync() {
        if (Listener is null || CTS is null || State == ServiceState.Stopping) return;
        try {
            var httpListenerContext = await Listener.GetContextAsync();
            if (httpListenerContext.Request.IsWebSocketRequest) {
                HttpListenerWebSocketContext httpListenerWebSocketContext;
                httpListenerWebSocketContext = await httpListenerContext.AcceptWebSocketAsync(Codec.SubProtocol ?? string.Empty);
                var context = new WebSocketContext(httpListenerWebSocketContext, httpListenerContext.Request);
                if (context.IsOpen) {
                    Codec.SessionProvider?.OpenSession(context.Transport);
                    Clients.Add(context);
                    await StartReceiveAsync(context, CTS.Token);
                    await OnClientConnectedAsync(new WebSocketEventArgs(context));
                }
            }
        }
        catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Invokes <see cref="ClientConnected"/> event if applicable.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual ValueTask OnClientConnectedAsync(WebSocketEventArgs e) => ClientConnected?.Invoke(this, e) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Invokes <see cref="ClientDisconnecting"/> event.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual ValueTask OnClientDisconnectingAsync(WebSocketEventArgs e) => ClientDisconnecting?.Invoke(this, e) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Invokes <see cref="ClientDisconnected"/> event.
    /// </summary>
    protected virtual void OnClientDisconnected(Guid? id) => ClientDisconnected?.Invoke(id);

    /// <summary>
    /// Called when the exception occurs during connecting a client.
    /// </summary>
    /// <param name="exception"></param>
    protected virtual ValueTask OnConnectExceptionAsync(Exception exception) {
        if (exception is HttpListenerException hlx && hlx.ErrorCode == 995)
            return ValueTask.CompletedTask; // connection aborted - it's normal on server shutdown.
        return ConnectException?.Invoke(this, new ExceptionEventArgs(exception)) ?? ValueTask.CompletedTask;
    }

    /// <summary>
    /// Additional task awaited after the successfull sign in.
    /// </summary>
    /// <param name="e">Event data.</param>
    /// <returns>A value task completed when the processing is done.</returns>
    protected virtual ValueTask OnUserSignedInAsync(WebSocketSessionEventArgs e) => UserSignedIn?.Invoke(this, e) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Additional task awaited when the authentication fails.
    /// </summary>
    /// <param name="e">Event data.</param>
    /// <returns>A value task completed when the processing is done.</returns>
    protected virtual ValueTask OnUserSignInFailAsync(WebSocketSessionEventArgs e) => UserSignInFail?.Invoke(this, e) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Additional task awaited when the sign out message is receive, right before the session is closed.
    /// </summary>
    /// <param name="e">Event data.</param>
    /// <returns>A value task completed when the processing is done.</returns>
    protected virtual ValueTask OnUserSigningOutAsync(WebSocketSessionEventArgs e) => UserSigningOut?.Invoke(this, e) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Invokes the <see cref="UserSignedOut"/> event.
    /// </summary>
    protected virtual void OnUserSignedOut(Guid? id) => UserSignedOut?.Invoke(id);

    #endregion

    #region Data fields

    /// <summary>
    /// Socket contexts of the clients currently connected to the server.
    /// </summary>
    private readonly List<WebSocketContext> Clients = [];

    /// <summary>
    /// A regular expression matching WebSocket protocol URI part.
    /// </summary>
    private readonly Regex RxWS = RxWSCT();

    /// <summary>
    /// A <see cref="HttpListener"/> used to listen for incomming connections.
    /// </summary>
    private HttpListener? Listener;

    [GeneratedRegex("^ws", RegexOptions.Compiled)]
    private static partial Regex RxWSCT();

    #endregion

}
