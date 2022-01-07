using System;
using System.Linq;
using System.Threading.Tasks;

using Test.Server.Data;

using Woof.Net;

namespace Test.Server;

public class TestAuthenticationProvider : IAuthenticationProvider {

    static readonly IClient TestClient = new Client { Id = Settings.Default.Test.ClientId };
    static readonly IUser TestUser = new User() { Id = Settings.Default.Test.UserId };

    public async ValueTask<byte[]?> GetKeyAsync(byte[] apiKey) {
        var testApiKey = Convert.FromBase64String(Settings.Default.Test.ApiKey!);
        await Task.Delay(1);
        var isKeyEqual = apiKey.AsSpan().SequenceEqual(testApiKey.AsSpan());
        return isKeyEqual ? Convert.FromBase64String(Settings.Default.Test.Secret!) : null;
    }

    public ValueTask<IClient?> GetClientAsync(Guid id)
        => ValueTask.FromResult(id == Settings.Default.Test.ClientId ? TestClient : null);

    public ValueTask<IUser?> GetUserAsync(byte[] apiKey)
        => ValueTask.FromResult(
            apiKey.AsSpan().SequenceEqual(Convert.FromBase64String(Settings.Default.Test.ApiKey!).AsSpan())
                ? TestUser
                : null
        );

}
