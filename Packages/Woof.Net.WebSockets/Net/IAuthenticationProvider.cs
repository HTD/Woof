namespace Woof.Net;

/// <summary>
/// Implement this to allow ansynchronous authentication of the API key.
/// </summary>
public interface IAuthenticationProvider {

    /// <summary>
    /// Gets the message signing key from the API key.
    /// </summary>
    /// <param name="apiKey">API key.</param>
    /// <returns>Task returning the message signing key or null if not authenticated.</returns>
    public ValueTask<byte[]?> GetKeyAsync(byte[] apiKey);

    /// <summary>
    /// Gets the client data for specified client identifier.
    /// </summary>
    /// <param name="id">Globally unique client identifier.</param>
    /// <returns>Task returning the client data for specified client identifier or null if not found.</returns>
    public ValueTask<IClient?> GetClientAsync(Guid id);

    /// <summary>
    /// Geths the user data associated to the API key.
    /// </summary>
    /// <param name="apiKey">API key.</param>
    /// <returns>Task returning the user data for specified API key or null if not found.</returns>
    public ValueTask<IUser?> GetUserAsync(byte[] apiKey);

}
