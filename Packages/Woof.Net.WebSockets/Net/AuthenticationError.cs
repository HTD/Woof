namespace Woof.Net;

/// <summary>
/// Authentication eror enumeration.
/// </summary>
[Flags]
public enum AuthenticationError {

    /// <summary>
    /// Authentication successfull.
    /// </summary>
    None = 0,

    /// <summary>
    /// Invalid API key or secret.
    /// </summary>
    ApiAccessDenied = 1,

    /// <summary>
    /// User access denied.
    /// </summary>
    UserAccessDenied = 2,

    /// <summary>
    /// Client access denied.
    /// </summary>
    ClientAccessDenied = 4

}

/// <summary>
/// Provides the descriptions for the <see cref="AuthenticationError"/> flags.
/// </summary>
public static class AuthenticationErrors {

    /// <summary>
    /// Gets the description of the authentication error code flags.
    /// </summary>
    /// <param name="error">Authentication error.</param>
    /// <returns>A short text description of the flags.</returns>
    public static string GetDescription(this AuthenticationError error) {
        Dictionary<AuthenticationError, string> descriptions = new() {
            [AuthenticationError.ApiAccessDenied] = "Invalid API key or secret",
            [AuthenticationError.UserAccessDenied] = "User access denied",
            [AuthenticationError.ClientAccessDenied] = "Client access denied"
        };
        return String.Join(" | ", descriptions.Where(d => error.HasFlag(d.Key)).Select(d => d.Value));
    }

}