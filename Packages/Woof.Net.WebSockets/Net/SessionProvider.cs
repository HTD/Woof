using System.Runtime.Serialization;

namespace Woof.Net;

/// <summary>
/// Provides session management for both client and server.
/// </summary>
/// <remarks>
/// <see cref="IAsyncTransport"/> is used as context object for generating identifiers
/// to allow the <see cref="SessionProvider"/> to be used with different transports
/// supported by <see cref="SubProtocolCodec"/>.
/// </remarks>
public sealed class SessionProvider : IDisposable {

    /// <summary>
    /// Gets the sessions collection for the server instance.
    /// </summary>
    public SessionCollection? Sessions { get; private set; }

    /// <summary>
    /// Initializes session identifier for the current client context.<br/>
    /// Switches <see cref="SessionProvider"/> to SERVER mode / multiple sessions.
    /// </summary>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    public void OpenSession(IAsyncTransport context) {
        IdGenerator ??= new ObjectIDGenerator();
        IdGenerator.GetId(context, out var _);
    }

    /// <summary>
    /// Removes and disposes session opened with <see cref="OpenSession(IAsyncTransport)"/>.
    /// </summary>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    public void CloseSession(IAsyncTransport context) {
        if (IdGenerator is null || Sessions is null) return;
        var sessionId = IdGenerator.GetId(context, out var firstTime);
        if (firstTime) throw new InvalidOperationException();
        Sessions.Remove(sessionId);
    }

    /// <summary>
    /// Gets a session for the current client connection.<br/>
    /// If the session doesn't exist it's created with an empty constructor.
    /// </summary>
    /// <typeparam name="TSession">Session type.</typeparam>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    /// <returns>Session object.</returns>
    /// <exception cref="NullReferenceException">Thrown when no context is provided for server.</exception>
    public TSession GetSession<TSession>(IAsyncTransport? context = null) where TSession : ISession, new() {
        if (IdGenerator is null) return (TSession)(Session = new TSession()); // single session, started from client scenario.
        if (context is null) throw new NullReferenceException("Context is required for server use");
        var sessionId = IdGenerator.GetId(context, out _);
        Sessions ??= new SessionCollection();
        if (!Sessions.ContainsKey(sessionId)) {
            var newSession = new TSession();
            Sessions.Add(sessionId, newSession);
            return newSession;
        }
        return (TSession)Sessions[sessionId];
    }

    /// <summary>
    /// Gets an existing session for the context.
    /// </summary>
    /// <param name="context">Connection context.</param>
    /// <returns>Session.</returns>
    public ISession? GetExistingSession(IAsyncTransport context) {
        if (IdGenerator is null || Sessions is null) return null;
        var sessionId = IdGenerator.GetId(context, out bool firstTime);
        return firstTime ? null : Sessions[sessionId];
    }

    /// <summary>
    /// Gets the message signing key associated with the current session.
    /// </summary>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    /// <returns>Message signing key.</returns>
    public byte[]? GetKey(IAsyncTransport context) {
        if (IdGenerator is null && Session is null) return null;
        if (IdGenerator is null) return Session?.Key;
        var sessionId = IdGenerator.GetId(context, out _);
        return Sessions != null && Sessions.TryGetValue(sessionId, out var session) ? session.Key : null;
    }

    /// <summary>
    /// Disposes and clears the sessions. The session provider remains usable.
    /// </summary>
    public void Dispose() {
        Sessions?.Dispose();
        if (Session is IDisposable disposableSession) disposableSession.Dispose();
    }

    /// <summary>
    /// Single client session.
    /// </summary>
    private ISession? Session;

    /// <summary>
    /// Object identifier generator instance.
    /// </summary>
    private ObjectIDGenerator? IdGenerator;

}
