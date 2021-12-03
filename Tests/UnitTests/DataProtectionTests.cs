using Woof.DataProtection;

namespace UnitTests;

public class DataProtectionTests {

    [Fact]
    public void Availability() => Assert.True(DP.IsAvailable);

    [Fact]
    public void CurrentUser() {
        var plain = "The quick brown fox jumps over the lazy dog.";
        var encrypted = DP.Protect(plain, DataProtectionScope.CurrentUser);
        var decrypted = DP.Unprotect(encrypted, DataProtectionScope.CurrentUser);
        Assert.Equal(plain, decrypted);
        Assert.NotEqual(plain, encrypted);
    }

    [Fact]
    public void LocalMachine() {
        var plain = "The quick brown fox jumps over the lazy dog.";
        var encrypted = DP.Protect(plain, DataProtectionScope.LocalMachine);
        var decrypted = DP.Unprotect(encrypted, DataProtectionScope.LocalMachine);
        Assert.Equal(plain, decrypted);
        Assert.NotEqual(plain, encrypted);
    }

}