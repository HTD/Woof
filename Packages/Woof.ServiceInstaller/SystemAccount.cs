namespace Woof.ServiceInstaller;

/// <summary>
/// Default windows system accounts used to run services.
/// </summary>
public enum SystemAccount {

    /// <summary>
    /// Default.
    /// </summary>
    LocalSystem = 0,

    /// <summary>
    /// Network service ("NT Authority\NetworkService").
    /// </summary>
    NetworkService = 1

}