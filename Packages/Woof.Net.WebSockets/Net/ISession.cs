namespace Woof.Net;

/// <summary>
/// A session having a message signing key.
/// </summary>
public interface ISession {

    /// <summary>
    /// Gets a message signing key for the session.
    /// </summary>
    byte[]? Key { get; set; }

}
