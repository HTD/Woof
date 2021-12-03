namespace Woof.Net;

/// <summary>
/// An interface for the user metadata.
/// </summary>
public interface IUser {

    /// <summary>
    /// Gets or sets the API user identifier.
    /// </summary>
    public Guid Id { get; set; }

}
