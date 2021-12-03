namespace Woof.Net;

/// <summary>
/// Contains user session data.
/// </summary>
public class UserSession : ISession {

    /// <summary>
    /// Gets or sets the message signing key (client secret).
    /// </summary>
    public byte[]? Key { get; set; }

    /// <summary>
    /// Gets or sets the session client data.
    /// </summary>
    public IClient? Client { get; set; }

    /// <summary>
    /// Gets or sets the session user data.
    /// </summary>
    public IUser? User { get; set; }

}