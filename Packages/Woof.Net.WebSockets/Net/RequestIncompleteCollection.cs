namespace Woof.Net;

/// <summary>
/// A collection used for matching response messages to the request messages.
/// </summary>
public sealed class RequestIncompleteCollection : Dictionary<Guid, ResponseSynchronizer>, IDisposable {

    /// <summary>
    /// Gets the subprotocol codec.
    /// </summary>
    private SubProtocolCodec Codec { get; }

    /// <summary>
    /// Gets a new response pack with a new identifier.
    /// </summary>
    public (Guid, ResponseSynchronizer) NewResponseSynchronizer {
        get {
            var id = Codec.NewId;
            var synchronizer = new ResponseSynchronizer();
            this[id] = synchronizer;
            return (id, synchronizer);
        }
    }

    /// <summary>
    /// Creates the collection for the specified codec.
    /// </summary>
    /// <param name="codec">Subprotocol codec.</param>
    public RequestIncompleteCollection(SubProtocolCodec codec) => Codec = codec;

    /// <summary>
    /// Try to remove the response synchronizer pointed with <paramref name="id"/> from the collection and return it.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="responseSynchronizer">Response synchronizer.</param>
    /// <returns>True, if the response synchronizer was stored in the collection.</returns>
    public bool TryRemoveResponseSynchronizer(Guid id, out ResponseSynchronizer? responseSynchronizer) {
        if (ContainsKey(id)) {
            responseSynchronizer = this[id];
            Remove(id);
            return true;
        }
        responseSynchronizer = null;
        return false;
    }

    /// <summary>
    /// Disposes all disposable values inside the collection and clears it. The collection remains usable.
    /// </summary>
    public void Dispose() {
        foreach (var i in Values) i.Dispose();
        Clear();
    }

}
