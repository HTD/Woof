namespace Woof.Net;

/// <summary>
/// A collection for storing and retrieving optionally disposable sessions.
/// </summary>
public sealed class SessionCollection : Dictionary<long, ISession>, IDisposable {

    /// <summary>
    /// Removes the session with given identifier if it exists in the collection.
    /// When the session implements <see cref="IDisposable"/> it will be disposed first.
    /// </summary>
    /// <param name="id">Session identifier.</param>
    public new void Remove(long id) {
        if (!ContainsKey(id)) return;
        if (this[id] is IDisposable disposable) disposable.Dispose();
        base.Remove(id);
    }

    /// <summary>
    /// Disposes all disposable sessions and clears the collection.
    /// </summary>
    public void Dispose() {
        foreach (var session in Values)
            if (session is IDisposable disposableSession) disposableSession.Dispose();
        Clear();
    }

}
