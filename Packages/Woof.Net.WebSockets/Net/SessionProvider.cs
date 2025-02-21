using System.Collections.Concurrent;

namespace Woof.Net;

/// <summary>
/// Provides session management for both client and server.
/// </summary>
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
        _ = GetSessionId(context); // Generates and stores a session ID for the context
    }

    /// <summary>
    /// Removes and disposes session opened with <see cref="OpenSession(IAsyncTransport)"/>.
    /// </summary>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    public void CloseSession(IAsyncTransport context) {
        if (Sessions is null) return;
        if (_sessionIds.TryRemove(context, out var sessionId)) {
            Sessions.Remove(sessionId);
        }
    }

    /// <summary>
    /// Gets a session for the current client connection.<br/>
    /// If the session doesn't exist, it's created with an empty constructor.
    /// </summary>
    /// <typeparam name="TSession">Session type.</typeparam>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    /// <returns>Session object.</returns>
    /// <exception cref="NullReferenceException">Thrown when no context is provided for server.</exception>
    public TSession GetSession<TSession>(IAsyncTransport? context = null) where TSession : ISession, new() {
        if (context is null) {
            // Single session, started from client scenario
            return (TSession)(Session ??= new TSession());
        }
        var sessionId = GetSessionId(context);
        Sessions ??= [];
        if (!Sessions.TryGetValue(sessionId, out ISession? value)) {
            var newSession = new TSession();
            value = newSession;
            Sessions.Add(sessionId, value);
            return newSession;
        }
        return (TSession)value;
    }

    /// <summary>
    /// Gets an existing session for the context.
    /// </summary>
    /// <param name="context">Connection context.</param>
    /// <returns>Session.</returns>
    public ISession? GetExistingSession(IAsyncTransport context) {
        if (Sessions is null || !_sessionIds.TryGetValue(context, out var sessionId)) {
            return null;
        }
        return Sessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    /// <summary>
    /// Gets the message signing key associated with the current session.
    /// </summary>
    /// <param name="context">Connection context to be used to generate the session identifier from.</param>
    /// <returns>Message signing key.</returns>
    public byte[]? GetKey(IAsyncTransport context) {
        if (Session is not null) return Session.Key;
        if (Sessions is null || !_sessionIds.TryGetValue(context, out var sessionId)) return null;
        return Sessions.TryGetValue(sessionId, out var session) ? session.Key : null;
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
    /// Generates or retrieves a session ID for the given context.
    /// </summary>
    /// <param name="context">Connection context.</param>
    /// <returns>Session ID.</returns>
    private long GetSessionId(IAsyncTransport context) {
        return _sessionIds.GetOrAdd(context, _ => Interlocked.Increment(ref _sessionIdCounter));
    }

    /// <summary>
    /// Dictionary to map connection contexts to session IDs.
    /// </summary>
    private readonly ConcurrentDictionary<IAsyncTransport, long> _sessionIds = new();

    /// <summary>
    /// Counter for generating unique session IDs.
    /// </summary>
    private long _sessionIdCounter;

}