namespace Woof.Net.Messages;

/// <summary>
/// Defines a generic error response with 32-bit error code and optional string description.
/// </summary>
public interface IGenericErrorResponse {

    /// <summary>
    /// Gets or sets an error code agreed between client and server to identify the error type. Zero for not-specified.
    /// </summary>
    int Code { get; set; }

    /// <summary>
    /// Gets or sets the optional error message / description.
    /// </summary>
    string? Description { get; set; }

}
