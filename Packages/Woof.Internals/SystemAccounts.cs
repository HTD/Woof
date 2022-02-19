namespace Woof.Internals;

/// <summary>
/// Default windows system accounts used to run services.
/// </summary>
public enum SystemAccount {

    /// <summary>
    /// Default. ("NT AUTHORITY\SYSTEM").
    /// </summary>
    LocalSystem = 0,

    /// <summary>
    /// Network service ("NT AUTHORITY\NETWORK SERVICE").
    /// </summary>
    NetworkService = 1

}

/// <summary>
/// System accounts names.
/// </summary>
public static class SystemAccounts {

    /// <summary>
    /// Gets the full account name with domain.
    /// </summary>
    /// <param name="account">Account enumeration value.</param>
    /// <returns>Full account name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Invalid enumeration value.</exception>
    public static string Name(SystemAccount account) => account switch {
        SystemAccount.LocalSystem => LocalSystem,
        SystemAccount.NetworkService => NetworkService,
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    /// <summary>
    /// Local System.
    /// </summary>
    public const string LocalSystem = @"NT AUTHORITY\SYSTEM";

    /// <summary>
    /// Network Service.
    /// </summary>
    public const string NetworkService = @"NT AUTHORITY\NETWORK SERVICE";

}