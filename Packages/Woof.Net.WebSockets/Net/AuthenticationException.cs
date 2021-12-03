
using Woof.Net.Messages;

namespace Woof.Net;

/// <summary>
/// Exception thrown when endpoint authentication failed.
/// </summary>
public class AuthenticationException : Exception {

    /// <summary>
    /// Gets the authentication error code.
    /// </summary>
    public AuthenticationError Code { get; }

    /// <summary>
    /// Creates <see cref="AuthenticationException"/> from <see cref="AuthenticationErrorResponse"/>.
    /// </summary>
    /// <param name="errorResponse">Error response.</param>
    public AuthenticationException(AuthenticationErrorResponse errorResponse)
        : base(errorResponse.Description) => Code = (AuthenticationError)errorResponse.Code;

    /// <summary>
    /// Creates <see cref="AuthenticationException"/> from <see cref="AuthenticationError"/>.
    /// </summary>
    /// <param name="authenticationError">Error.</param>
    public AuthenticationException(AuthenticationError authenticationError)
        : base(AuthenticationErrors.GetDescription(authenticationError)) => Code = authenticationError;

}