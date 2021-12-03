namespace Woof.Net.Messages;

/// <summary>
/// Implement in sign in request to make the request automatically match the message signing key with the API key.
/// </summary>
public interface ISignInRequest {

    /// <summary>
    /// Gets the API key sent in the request.
    /// </summary>
    public byte[] ApiKey { get; }

}
