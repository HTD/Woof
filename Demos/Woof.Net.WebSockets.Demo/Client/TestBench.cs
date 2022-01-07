using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Test.Api;

using Woof.ConsoleTools;
using Woof.Net;
using Woof.Net.WebSockets;

namespace Test.Client;

/// <summary>
/// Client-server communication tests and documentation in one.
/// </summary>
class TestBench {

    #region Public API

    /// <summary>
    /// Creates tests for the API client.
    /// </summary>
    /// <param name="client">WS API client.</param>
    public TestBench(TestClient client) => Client = client;

    /// <summary>
    /// Starts the tests.
    /// </summary>
    /// <returns>Task completed when all test are completed or failed.</returns>
    public async ValueTask StartAsync() {
        ConsoleEx.Init();
        ConsoleEx.AssemblyHeader<WebSocketContext>(HeaderItems.All);
        ConsoleEx.Header("Initialization:");
        ConsoleEx.AlignStart = 40;
        var startCursor = ConsoleEx.Start("Connecting to server");
        try {
            if (Client.State != ServiceState.Started) await Client.StartAsync();
            ConsoleEx.Complete(startCursor);
            Console.WriteLine(ConsoleEx.SeparatorLine);
            ConsoleEx.Header("Self-test:");
            var tests = TestItem.GetTests(this);
            foreach (var test in tests) test.Cursor = ConsoleEx.Start(test.Name);
            Console.WriteLine(ConsoleEx.SeparatorLine);
            foreach (var test in tests) await test.StartAsync();
        }
        catch (Exception exception) {
            ConsoleEx.Complete(startCursor, false);
            while (exception.InnerException != null) exception = exception.InnerException;
            ConsoleEx.Log('e', exception.Message);
        }
    }

    #endregion

    #region Tests

    [Test]
    public async ValueTask<string> InitialPingAsync() {
        var time = await Client.PingAsync();
        return $"Time: {time:s\\.fff}s";
    }

    [Test("Subsequent ping")]
    public async ValueTask<string> SubsequentPingAsync() {
        var time = await Client.PingAsync();
        return $"Time: {time.TotalMilliseconds}ms";
    }

    [Test("Unauthorized call")]
    public ValueTask<string> UnauthorizedCall1Async() => UnauthorizedAsync();

    [Test("Sign in: invalid client id")]
    public ValueTask<string> SignInInvalidClientIdAsync() => SignInInvalidAsync(corruptId: true);

    [Test("Sign in: valid")]
    public ValueTask SignInValidAsync() => SignInAsync();

    [Test("Authorized call")]
    public ValueTask AuthorizedCall1Async() => AuthorizedCallAsync();

    [Test("Sign in: invalid key")]
    public ValueTask<string> SignInInvalidKeyAsync() => SignInInvalidAsync(corruptKey: true);

    [Test("Sign in: invalid secret")]
    public ValueTask<string> SignInInvalidSecretAsync() => SignInInvalidAsync(corruptSecret: true);

    [Test("Unauthorized call")]
    public ValueTask<string> UnauthorizedCall2Async() => UnauthorizedAsync();

    [Test("Valid division")]
    public async ValueTask ValidDivisionAsync() {
        var result = await Client.DivideAsync(1, 2);
        if (result != 0.5m) throw new InvalidOperationException("Invalid result");
    }

    [Test("Invalid division")]
    public async ValueTask<string> InvalidDivisionAsync() {
        decimal result;
        try {
            result = await Client.DivideAsync(1, 0);
        }
        catch (Exception x) {
            return $"{x.GetType().Name}: {x.Message}";
        }
        throw new InvalidOperationException($"Got result: {result}");
    }

    [Test("Unknown message")]
    public async ValueTask UnknownMessageAsync() {
        const int typeId = 0x0666;
        byte[] data = Encoding.ASCII.GetBytes("SURPRISE! Nobody expects the Spanish Inquisition");
        var response = await Client.RawMessageAsync(typeId, data);
        var isValid = response.TypeId == typeId && response.Data.SequenceEqual(data);
        if (!isValid) throw new InvalidOperationException("Invalid response");
    }

    [Test("Message ignoring")]
    public async ValueTask MessageIgnoringAsync() {
        var timeoutDefault = Client.Timeout;
        Client.Timeout = TimeSpan.FromSeconds(0.25);
        await Client.IgnoreRequestsAsync(2);
        var timeOutCount = 0;
        for (var i = 0; i < 3; i++) {
            try {
                await Client.PingAsync();
            }
            catch (TimeoutException) {
                timeOutCount++;
            }
        }
        Client.Timeout = timeoutDefault;
        if (timeOutCount != 2) throw new InvalidOperationException();
    }

    [Test("Complex array")]
    public async ValueTask ComplexArrayAsync() {
        var data = await Client.ComplexArrayAsync();
        if (data.Length != 2) throw new InvalidOperationException();
    }

    [Test("Stream download")]
    public async ValueTask StreamDownloadAsync() {
        var id = Settings.Default.Test.StreamId;
        var testStream = Helpers.GetTestDataStream(Settings.Default.Test.StreamLength, Settings.Default.Test.StreamSeed);
        var testData = new Memory<byte>(new byte[testStream.Length]);
        await testStream.ReadAsync(testData);
        var testPosition = 42;
        var testFragmentLength = 1024;
        var testFragment = testData.Slice(testPosition, testFragmentLength);
        ReadOnlyMemory<byte> readData;
        readData = await Client.DownloadAsync(id, testFragment.Length, testPosition);
        if (!testFragment.Span.SequenceEqual(readData.Span)) throw new InvalidOperationException("Read fragment has invalid content");
        readData = await Client.DownloadAsync(id, 0, 0);
        if (!testData.Span.SequenceEqual(readData.Span)) throw new InvalidOperationException("Read stream has invalid content");
    }

    [Test("Disconnect on sign in failed")]
    public async ValueTask DisconnectOnInvalidSignInAsync() {
        await Client.SendMessageAsync(new StrictSignInPolicyRequest { DisconnectOnAuthenticationFailed = true });
        await SignInInvalidAsync(corruptSecret: true);
        await Client.WaitStateAsync(ServiceState.Stopped, TimeSpan.FromSeconds(2));
        if (Client.State != ServiceState.Stopped) throw new InvalidOperationException($"Invalid client state: {Client.State}");
    }

    [Test("Server restart")]
    public async ValueTask ServerRestartAsync() {
        await Client.StartAsync();
        await Client.RestartServerAsync();
        await Client.WaitStateAsync(ServiceState.Stopped, TimeSpan.FromSeconds(2));
        if (Client.State != ServiceState.Stopped) throw new InvalidOperationException($"Invalid client state: {Client.State}");
        await Client.StartAsync();
        await Client.WaitStateAsync(ServiceState.Started, TimeSpan.FromSeconds(2));
        if (Client.State != ServiceState.Started) throw new InvalidOperationException($"Invalid client state: {Client.State}");
    }

    [Test("Server ID")]
    public ValueTask IdTestAsync() => IdAsync();

    [Test("Query API")]
    public ValueTask QueryApiTestAsync() => QueryApiAsync();

    [Test("Subscribe to server time")]
    public ValueTask SubscriptionTestAsync() => Client.TimeSubscribeAsync(TimeSpan.FromSeconds(10));

    #endregion

    #region Helpers

    /// <summary>
    /// Tested client.
    /// </summary>
    private readonly TestClient Client;

    /// <summary>
    /// Sings in to the server with configured credentials.
    /// </summary>
    /// <param name="corruptId">Set to corrupt client identifier.</param>
    /// <param name="corruptKey">Set to corrupt API key.</param>
    /// <param name="corruptSecret">Set to corrupt API secret.</param>
    /// <returns>Task completed when authentication completes or fails.</returns>
    async ValueTask SignInAsync(bool corruptId = false, bool corruptKey = false, bool corruptSecret = false) {
        var id = Settings.Default.Credentials.ClientId;
        var key = Settings.Default.Credentials.ApiKey;
        var secret = Settings.Default.Credentials.Secret;
        if (key is null || secret is null) throw new NullReferenceException();
        if (corruptId) id = Guid.NewGuid();
        if (corruptKey) key = Helpers.CorruptBase64Key(key);
        if (corruptSecret) secret = Helpers.CorruptBase64Key(secret);
        await Client.SignInAsync(id, key, secret);
    }

    /// <summary>
    /// Sings in to the server with configured credentials, but expecting it to fail.
    /// </summary>
    /// <param name="corruptId">Set to corrupt client identifier.</param>
    /// <param name="corruptKey">Set to corrupt API key.</param>
    /// <param name="corruptSecret">Set to corrupt API secret.</param>
    /// <returns>Task returning authentication error message.</returns>
    /// <exception cref="InvalidOperationException">Thrown when invalid exception is received or the access is granted.</exception>
    async ValueTask<string> SignInInvalidAsync(bool corruptId = false, bool corruptKey = false, bool corruptSecret = false) {
        try {
            await SignInAsync(corruptId, corruptKey, corruptSecret);
        }
        catch (AuthenticationException x) {
            return $"{x.GetType().Name}: {x.Message}" ?? "NO MESSAGE!";
        }
        catch (Exception x) {
            throw new InvalidOperationException($"Invalid exception: {x.GetType().Name}");
        }
        throw new InvalidOperationException("Granted");
    }

    /// <summary>
    /// Performs an authorized call.
    /// </summary>
    /// <returns>Task completed when the test passes or fails.</returns>
    /// <exception cref="InvalidOperationException">Thrown when invalid response is received.</exception>
    async ValueTask AuthorizedCallAsync() {
        var response = await Client.CheckAuthorizedAsync();
        if (response != "AUTHORIZED") throw new InvalidOperationException($"Invalid response: {response}");
    }

    /// <summary>
    /// Performs an unauthorized call and expects it to fail.
    /// </summary>
    /// <returns>Task returning the exception message.</returns>
    /// <exception cref="InvalidOperationException">Thrown when access granted.</exception>
    async ValueTask<string> UnauthorizedAsync() {
        string? response;
        try {
            response = await Client.CheckAuthorizedAsync();
        }
        catch (Exception x) {
            return $"{x.GetType().Name}: {x.Message}";
        }
        if (response == "AUTHORIZED") throw new InvalidOperationException("Granted");
        throw new InvalidOperationException("No data, but no exception thrown");
    }

    /// <summary>
    /// Loads and displays the server identification data.
    /// </summary>
    /// <returns>Task completed when server identification data is fetched and displayed.</returns>
    async ValueTask IdAsync() {
        var id = await Client.IdAsync();
        ConsoleEx.Header("Server id:");
        ConsoleEx.Item($"Name: {id.Name}");
        ConsoleEx.Item($"Version: {id.Version}");
        ConsoleEx.Item($"Build time: {id.BuildTime}");
        ConsoleEx.Item($"Up time: {id.UpTime}");
        ConsoleEx.Item($"Timeout: {id.Timeout}");
        Console.WriteLine(ConsoleEx.SeparatorLine);
    }

    /// <summary>
    /// Queries the API for messages and displays them all.
    /// </summary>
    /// <returns>Task completed when messages fetched and displayed.</returns>
    async ValueTask QueryApiAsync() {
        await SignInAsync();
        var api = await Client.QueryApiAsync(noInternals: true);
        ConsoleEx.Header("API messages:");
        foreach (var msg in api) {
            ConsoleEx.Item($"{msg.Key:X8} :: {msg.Value}");
        }
        await Client.SignOutAsync();
        Console.WriteLine(ConsoleEx.SeparatorLine);
    }

    #endregion

}
