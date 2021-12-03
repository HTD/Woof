namespace Woof.DataProtection.Api;

/// <summary>
/// Unsupported OS API.
/// </summary>
internal class Unsupported : IDPAPI {

    /// <summary>
    /// This API is not supported so obviously not operational.
    /// </summary>
    public bool IsOperational => false;

    /// <summary>
    /// Just throws the <see cref="PlatformNotSupportedException"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public byte[] Protect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => throw new PlatformNotSupportedException();

    /// <summary>
    /// Just throws the <see cref="PlatformNotSupportedException"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public byte[] Unprotect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => throw new PlatformNotSupportedException();

}