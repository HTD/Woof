namespace Woof.Net;

/// <summary>
/// An interface for the client metadata.
/// </summary>
public interface IClient {

    /// <summary>
    /// Gets or sets the API client identifier.
    /// </summary>
    public Guid Id { get; set; }

}
