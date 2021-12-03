namespace Woof.Net;

/// <summary>
/// Basic client session data, provides secret key for the client.
/// </summary>
public class ClientSession : ISession {

    /// <summary>
    /// Gets or sets a message signing key for the session.
    /// </summary>
    public byte[]? Key { get; set; }

}
