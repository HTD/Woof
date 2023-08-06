using System;
using System.IO;
using System.Threading.Tasks;

using Test.Api;

using Woof.Net;
using Woof.Net.Messages;
using Woof.Net.WebSockets;

namespace Test.Server;

/// <summary>
/// Test server designed as a part of package documentation.
/// </summary>
public class TestServer : Server<WoofCodec> {

    private int IgnoreMessagesCount { get; set; }

    private TimeSpan LagTime { get; set; }

    private Stream? TestStream;

    protected override Stream GetStreamById(Guid id)
        => id == Settings.Default.Test.StreamId
            ? (TestStream ??= Helpers.GetTestDataStream(Settings.Default.Test.StreamLength, Settings.Default.Test.StreamSeed))
            : throw new InvalidOperationException($"Stream identified with {id} is unavailable");

    /// <summary>
    /// Initializes the test server instance.
    /// </summary>
    public TestServer() {
        Codec.AuthenticationProvider = new TestAuthenticationProvider();
        EndPointUri = Settings.Default.EndPointUri;
        Codec.LoadMessageTypes<Loader>();
    }

    /// <summary>
    /// Handles MessageReceived events.<br/>
    /// Note that since the function is "async void" it should not throw because such exceptions can't be caught.
    /// </summary>
    /// <param name="decodeResult">Message receive result.</param>
    /// <param name="context">WebSocket (client) context.</param>
    protected override async ValueTask OnMessageReceivedAsync(DecodeResult decodeResult, WebSocketContext context) {
        if (Codec.SessionProvider?.GetSession<UserSession>(context.Transport)?.Client?.Id is Guid clientId) {
            // This part tests if we can get the client's context knowing its id.
            // When there's only one client and it's signed in, it should find its own id.
            var clientSession = GetClientSession(clientId);
            var clientContext = GetClientContext(clientSession) ?? throw new InvalidOperationException("GetClientContext() failed");
        }
        if (LagTime > TimeSpan.Zero) await Task.Delay(LagTime);
        if (IgnoreMessagesCount > 0) {
            IgnoreMessagesCount--;
            return;
        }
        await base.OnMessageReceivedAsync(decodeResult, context);
        if (decodeResult.IsUnauthorized) {
            await SendMessageAsync(new E401_UnauthorizedResponse(), context, decodeResult.MessageId);
            return;
        }
        switch (decodeResult.Message) {
            case DivideRequest divideRequest:
                var result = divideRequest.X / divideRequest.Y; // made to throw, both server and client must handle this!
                await SendMessageAsync(new DivideResponse { Result = result }, context, decodeResult.MessageId);
                break;
            case PrivateRequest privateRequest:
                await SendMessageAsync(new PrivateResponse { Secret = "AUTHORIZED" }, context, decodeResult.MessageId);
                break;
            case TimeSubscribeRequest subscribeRequest:
                await AsyncLoop.FromIterationAsync(async () => {
                    var boxedMsg = new TimeNotification { Time = DateTime.Now };
                    await SendMessageAsync(boxedMsg, typeHint: null, context);
                    await Task.Delay(subscribeRequest.Period);
                }, OnReceiveException, () => context.IsOpen, false, CancellationToken);
                break;
            case ComplexArray complexArray:
                await SendMessageAsync(new ComplexArray { Items = complexArray.Items }, context, decodeResult.MessageId);
                break;
            case RawMessageRequest testUnexpectedRequest:
                await Codec.SendBinaryAsync(context.Transport, new MessageTypeContext(testUnexpectedRequest.TypeId, typeof(object)), testUnexpectedRequest.Data, default, CancellationToken);
                await SendMessageAsync(new RawMessageResponse { TypeId = testUnexpectedRequest.TypeId, Data = testUnexpectedRequest.Data }, context, decodeResult.MessageId);
                break;
            case IgnoreMessagesRequest ignoreMessagesRequest:
                IgnoreMessagesCount = ignoreMessagesRequest.Number;
                break;
            case IntroduceLagRequest introduceLagRequest:
                LagTime = introduceLagRequest.Time;
                break;
            case StrictSignInPolicyRequest strictSignInPolicyRequest:
                DisconnectOnAuthenticationFailed = strictSignInPolicyRequest.DisconnectOnAuthenticationFailed;
                break;
            case RestartRequest restartRequest:
                await StopAsync();
                await StartAsync();
                break;
        }

    }

}
