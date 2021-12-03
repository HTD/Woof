using Woof.Net.Messages;
using System.Reflection;
using System.Net;

namespace Woof.Net.WebSockets;

/// <summary>
/// WebSocket end point base class to be used by both clients and servers.
/// </summary>
/// <typeparam name="TCodec">Message codec implementing the subprotocol.</typeparam>
public abstract class WebSocketEndpoint<TCodec> : IDisposable where TCodec : SubProtocolCodec, new() {

    #region Public API

    #region Events

    /// <summary>
    /// Occurs when a message is received by the socket and handled by <see cref="OnMessageReceivedAsync(DecodeResult, WebSocketContext)"/>.
    /// </summary>
    /// <remarks>
    /// Does not block the receiving thread.
    /// </remarks>
    public event AsyncEventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Occurs when an exception is thrown during receive process.
    /// </summary>
    public event AsyncEventHandler<ExceptionEventArgs>? ReceiveException;

    /// <summary>
    /// Occurs when the state of the client or server service is changed.
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the subprotocol codec.
    /// </summary>
    public TCodec Codec { get; }

    /// <summary>
    /// Gets the WebSocket end point URI.
    /// </summary>
    public Uri? EndPointUri { get; set; }

    /// <summary>
    /// Gets the current endpoint state.
    /// </summary>
    public ServiceState State { get; set; } = ServiceState.Stopped;

    /// <summary>
    /// Gets or sets the operation timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

    #endregion

    /// <summary>
    /// Initializes the transport with the codec and request completion instances.
    /// </summary>
    public WebSocketEndpoint() {
        Codec = new TCodec { SessionProvider = new SessionProvider() };
        RequestsIncomplete = new RequestIncompleteCollection(Codec);
    }

    /// <summary>
    /// Waits until the service state of the endpoint changes to the requested state.
    /// </summary>
    /// <param name="state">Requested state.</param>
    /// <param name="time">Time to wait.</param>
    /// <returns>Task completed when the state changes or the timeout occurs.</returns>
    public async ValueTask WaitStateAsync(ServiceState state, TimeSpan time = default) {
        var t0 = DateTime.Now;
        while (DateTime.Now - t0 < time) {
            await StateChangedSignal.WaitAsync(time);
            if (State == state) break;
        }
    }

    #endregion

    #region Protected state

    /// <summary>
    /// Gets the cancellation token used for the client or server instance.
    /// </summary>
    protected CancellationToken CancellationToken => CTS?.Token ?? CancellationToken.None;

    /// <summary>
    /// A collection of incomplete requests requiring the other party's response.
    /// </summary>
    protected RequestIncompleteCollection RequestsIncomplete { get; }

    #endregion

    #region Virtuals

    /// <summary>
    /// Gets the limit of the message size that can be received (default 1MB).<br/>
    /// Override to zero or a negative number to remove the limitation (unsafe).<br/>
    /// </summary>
    public virtual int MaxReceiveMessageSize => 0x00100000; // 1MB

    /// <summary>
    /// Invokes <see cref="MessageReceived"/> event.
    /// </summary>
    /// <param name="decodeResult">Message decoding result.</param>
    /// <param name="context">WebSocket context, that can be used to send the response to.</param>
    /// <returns>Task completed when the message handling is done.</returns>
    protected virtual ValueTask OnMessageReceivedAsync(DecodeResult decodeResult, WebSocketContext context)
        => MessageReceived?.Invoke(this, new MessageReceivedEventArgs(decodeResult, context)) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Invokes <see cref="ReceiveException"/> event.
    /// </summary>
    /// <param name="exception">Exception passed.</param>
    protected virtual ValueTask OnReceiveException(Exception exception)
        => ReceiveException?.Invoke(this, new ExceptionEventArgs(exception)) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Called when the main receive loop is completed.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    /// <returns>Task completed immediately or when the aditional processing is done.</returns>
    protected virtual ValueTask OnReceiveEndAsync(WebSocketContext context) => ValueTask.CompletedTask;

    /// <summary>
    /// Invokes <see cref="StateChanged"/> event.
    /// </summary>
    protected virtual void OnStateChanged(ServiceState state) {
        StateChangedSignal.Set();
        StateChanged?.Invoke(this, new StateChangedEventArgs(state));
    }

    #endregion

    #region Transport tools

    /// <summary>
    /// Reads binary messages from the socket, deserializes them and triggers <see cref="MessageReceived"/> event.
    /// </summary>
    /// <param name="state">WebSocket context boxed.</param>
    /// <returns>Receive loop task.</returns>
    private async ValueTask Receive(object? state) {
        if (state is not WebSocketContext context) throw new ArgumentException($"State must be a {nameof(WebSocketContext)}", nameof(state));
        if (State != ServiceState.Started) {
            State = ServiceState.Started;
            TimeStarted = DateTime.Now;
            OnStateChanged(State);
        }
        GC.Collect();
        try {
            while (!CancellationToken.IsCancellationRequested && context.IsOpen) {
                try {
                    var decodeResult = await Codec.DecodeMessageAsync(context.Transport, MaxReceiveMessageSize, CancellationToken);
                    if (decodeResult is null) continue; // we should ignore empty frames, shouldn't we?
                    if (!decodeResult.IsSuccess && decodeResult.Exception?.Message != null) throw decodeResult.Exception;
                    if (decodeResult.IsCloseFrame) break;
                    if (RequestsIncomplete.TryRemoveResponseSynchronizer(decodeResult.MessageId, out var responseSynchronizer) && responseSynchronizer != null) {
                        responseSynchronizer.Message = decodeResult.Message;
                        if (responseSynchronizer.Semaphore != null && responseSynchronizer.Semaphore.CurrentCount < 1) responseSynchronizer.Semaphore.Release();
                    }
                    else if (decodeResult.TypeContext?.IsError == true && decodeResult.MessageId == default) {
                        RequestsIncomplete.Dispose(); // emergency release of incomplete requests on unmatched server error messages
                    }
                    else {
                        try {
                            await OnMessageReceivedAsync(decodeResult, context);
                        }
                        catch (Exception messageHandlerException) {
                            await SendMessageAsync(new E500_InternalServerErrorResponse {
                                Code = 500,
                                HResult = messageHandlerException.HResult,
                                Description = messageHandlerException.Message
                            }, context, decodeResult.MessageId);
                        }
                    }
                }
                catch (Exception exception) {
                    while (exception.InnerException is not null) exception = exception.InnerException;
                    if (exception is TaskCanceledException ||
                        exception is HttpListenerException httpListenerException && httpListenerException.NativeErrorCode == 995)
                        break;
                    await OnReceiveException(exception);
                    if (exception.Data.Contains("limit")) break;
                }
            }
        }
        finally {
            await OnReceiveEndAsync(context);
        }
    }

    /// <summary>
    /// Starts the receiving task for the specified context.
    /// </summary>
    /// <param name="context">WebSocket context.</param>
    /// <param name="token">Cancellation token used to cancel the task created.</param>
    /// <returns>Created task.</returns>
    protected Task StartReceiveAsync(WebSocketContext context, CancellationToken token = default)
        => Task.Factory.StartNew(
                Receive, context,
                token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    /// <summary>
    /// Serializes and sends a message to the specified context.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <param name="typeHint">Type hint.</param>
    /// <param name="context">Target context.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <returns>Task completed when the sending is done.</returns>
    protected ValueTask SendMessageAsync(object message, Type? typeHint, WebSocketContext context, Guid id = default)
        => Codec.SendEncodedAsync(context.Transport, message, typeHint, id, CancellationToken);

    /// <summary>
    /// Serializes and sends a message to the specified context.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <param name="message">Message to send.</param>
    /// <param name="context">Target context.</param>
    /// <param name="id">Optional message identifier, if not set - new unique identifier will be used.</param>
    /// <returns>Task completed when the sending is done.</returns>
    protected ValueTask SendMessageAsync<TMessage>(TMessage message, WebSocketContext context, Guid id = default)
        => Codec.SendEncodedAsync(context.Transport, message, id, CancellationToken);

    /// <summary>
    /// Sends a message to the specified context and awaits until the response of the specified type is received.
    /// </summary>
    /// <param name="request">Request message.</param>
    /// <param name="context">Target context.</param>
    /// <param name="timeout">Timeout value. Zero to for indefinite waiting.</param>
    /// <returns>Task returning the response message.</returns>
    /// <exception cref="UnexpectedMessageException">Thrown when a defined, but unexpected type message is received instead of expected one.</exception>
    /// <exception cref="TaskCanceledException">Thrown when the client or server operation is cancelled.</exception>
    protected async ValueTask<object?> SendAndReceiveAsync(object request, WebSocketContext context, TimeSpan timeout = default) {
        var (id, synchronizer) = RequestsIncomplete.NewResponseSynchronizer;
        try {
            await SendMessageAsync(request, typeHint: null, context, id);
            await synchronizer.Semaphore.WaitAsync(timeout);
            return synchronizer.Message;
        }
        finally {
            synchronizer.Dispose();
        }
    }

    /// <summary>
    /// Sends a message to the specified context and awaits until the response of the specified type is received.
    /// </summary>
    /// <typeparam name="TRequest">Request message type.</typeparam>
    /// <typeparam name="TResponse">Response message type.</typeparam>
    /// <param name="request">Request message.</param>
    /// <param name="context">Target context.</param>
    /// <param name="timeout">Timeout value. Zero to for indefinite waiting.</param>
    /// <returns>Task returning the response message.</returns>
    /// <exception cref="UnexpectedMessageException">Thrown when a defined, but unexpected type message is received instead of expected one.</exception>
    /// <exception cref="TaskCanceledException">Thrown when the client or server operation is cancelled.</exception>
    protected async ValueTask<TResponse> SendAndReceiveAsync<TRequest, TResponse>(TRequest request, WebSocketContext context, TimeSpan timeout = default) {
        var (id, synchronizer) = RequestsIncomplete.NewResponseSynchronizer;
        try {
            await SendMessageAsync(request, context, id);
            await synchronizer.Semaphore.WaitAsync(timeout == default ? Timeout : timeout);
            return synchronizer.Message is TResponse response
                ? response
                : throw WebSocketEndpoint<TCodec>.GetExceptionForUnexpectedMessage(synchronizer.Message);
        }
        finally {
            synchronizer.Dispose();
        }
    }

    /// <summary>
    /// Match the appropriate exception from unexpected message type.
    /// </summary>
    /// <param name="message">Message received instead of expected message type.</param>
    /// <returns>Exception to be thrown by <see cref="SendAndReceiveAsync{TRequest, TResponse}(TRequest, WebSocketContext, TimeSpan)"/> method.</returns>
    private static Exception GetExceptionForUnexpectedMessage(object? message) =>
        message switch {
            null => new TimeoutException("No response received within timeout period"),
            AuthenticationErrorResponse authenticationErrorResponse => new AuthenticationException(authenticationErrorResponse),
            E401_UnauthorizedResponse => new UnauthorizedAccessException(),
            E500_InternalServerErrorResponse errorResponse => errorResponse.HResult != default
                ? WebSocketEndpoint<TCodec>.GetExceptionForHResult(errorResponse.HResult, errorResponse.Description)
                : new InvalidOperationException(errorResponse.Description),
            _ => new UnexpectedMessageException(message)
        };

    /// <summary>
    /// Gets the correct type of exception from HRESULT code.
    /// </summary>
    /// <param name="hResult">HRESULT code.</param>
    /// <param name="message">Exception message.</param>
    /// <returns>Exception instance.</returns>
    private static Exception GetExceptionForHResult(int hResult, string? message) {
        var instance = Marshal.GetExceptionForHR(hResult);
        if (instance is null) return new InvalidOperationException(message);
        var type = instance.GetType();
        var messageField = type.GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
        messageField?.SetValue(instance, message);
        return instance;
    }

    /// <summary>
    /// Disposes <see cref="StateChangedSignal"/>, <see cref="CTS"/> and <see cref="RequestsIncomplete"/>.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes <see cref="StateChangedSignal"/>, <see cref="CTS"/> and <see cref="RequestsIncomplete"/>
    /// if called deterministically from <see cref="Dispose()"/>, not the finalizer.
    /// </summary>
    /// <param name="disposing">False if called from finalizer.</param>
    protected virtual void Dispose(bool disposing) {
        Codec?.SessionProvider?.Dispose();
        RequestsIncomplete?.Dispose();
        if (disposing) {
            StateChangedSignal.Dispose();
            CTS?.Dispose();
        }
    }

    #endregion

    #region Data fields

    /// <summary>
    /// A cancellation token source used to cancel all the client and server tasks.
    /// </summary>
    protected CancellationTokenSource? CTS;

    /// <summary>
    /// A time the server was last started.
    /// </summary>
    protected DateTime TimeStarted;

    /// <summary>
    /// A semaphore used for <see cref="WaitStateAsync(ServiceState, TimeSpan)"/>.
    /// </summary>
    private readonly AutoResetEventAsync StateChangedSignal = new();

    #endregion

}
