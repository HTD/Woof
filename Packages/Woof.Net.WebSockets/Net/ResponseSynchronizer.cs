namespace Woof.Net;

/// <summary>
/// A special container to get the response matched to the request
/// and the semaphore used to synchronize sending thread with the receiving thread.
/// </summary>
public class ResponseSynchronizer : IDisposable {

    /// <summary>
    /// Gets or sets the message object.
    /// </summary>
    public object? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets the semaphore used to synchronize the sending and receiving threads.
    /// </summary>
    public SemaphoreSlim Semaphore { get; }

    /// <summary>
    /// Creates new, empty response pack.
    /// </summary>
    public ResponseSynchronizer() => Semaphore = new SemaphoreSlim(0, 1);

    private bool IsDisposed;

    /// <summary>
    /// Disposes the underlying semaphore, releases it if applicable first, not to block awaiting sending thread.
    /// </summary>
    public void Dispose() {
        if (IsDisposed) return;
        try {
            if (Semaphore.CurrentCount < 1) Semaphore.Release();
            Semaphore.Dispose();
        }
        catch (ObjectDisposedException) {
            // we really don't care if the semaphore is already disposed by external code.
        }
        finally { // however:
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

}
