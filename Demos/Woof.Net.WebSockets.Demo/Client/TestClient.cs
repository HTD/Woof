using System;
using System.Threading.Tasks;

using Test.Api;

using Woof.Net;
using Woof.Net.WebSockets;

namespace Test.Client;

/// <summary>
/// Test client designed as a part of package documentation.
/// </summary>
public class TestClient : Client<WoofCodec> {

    public override int MaxReceiveMessageSize => 0x20_0000; // 2MB to make sure the download succeeds.

    /// <summary>
    /// Initializes the test server instance.
    /// </summary>
    public TestClient() {
        EndPointUri = Settings.Default.EndPointUri;
        Id = Settings.Default.Credentials.ClientId;
        Timeout = TimeSpan.FromSeconds(Settings.Default.Timeout);
        Codec.LoadMessageTypes<Loader>();
    }

    /// <summary>
    /// Ask the server to send a unexpected message type with specified type and raw binary data before it sends the valid response.
    /// </summary>
    /// <returns>Task returning cloned message data.</returns>
    public async Task<RawMessageResponse> RawMessageAsync(int typeId, byte[] data)
        => await SendAndReceiveAsync<RawMessageRequest, RawMessageResponse>(new RawMessageRequest { TypeId = typeId, Data = data });

    /// <summary>
    /// Performs a test division of two decimal numbers.
    /// </summary>
    /// <param name="x">Divident.</param>
    /// <param name="y">Divisor.</param>
    /// <returns>Taks returning a decimal result.</returns>
    /// <exception cref="UnexpectedMessageException">Thrown when the operation fails.</exception>
    public async Task<decimal> DivideAsync(decimal x, decimal y)
        => (await SendAndReceiveAsync<DivideRequest, DivideResponse>(new DivideRequest { X = x, Y = y })).Result;

    /// <summary>
    /// Performs a test private (authorized) request and returns a test value.
    /// </summary>
    /// <returns>Task returning string "AUTHORIZED" if successful.</returns>
    /// <exception cref="UnexpectedMessageException">Thrown when unauthorized, should contain <see cref="AccessDeniedResponse"/> message.</exception>
    public async Task<string> CheckAuthorizedAsync() 
        => (await SendAndReceiveAsync<PrivateRequest, PrivateResponse>(new PrivateRequest { ClientTime = DateTime.Now })).Secret;

    /// <summary>
    /// Subsribes to server time notification.
    /// </summary>
    /// <param name="period">Time period between notifications.</param>
    /// <returns>Task completed as soon as the subscription message is sent.</returns>
    public async ValueTask TimeSubscribeAsync(TimeSpan period = default)
        => await SendMessageAsync(new TimeSubscribeRequest { Period = period == default ? TimeSpan.FromSeconds(1) : period });

    /// <summary>
    /// Asks the server to ignore the next n messages.
    /// </summary>
    /// <param name="numberOfRequestsToIgnore">The number of client messages to ignore.</param>
    /// <returns>Task completed when the request is sent.</returns>
    public async ValueTask IgnoreRequestsAsync(int numberOfRequestsToIgnore)
        => await SendMessageAsync(new IgnoreMessagesRequest { Number = numberOfRequestsToIgnore });

    /// <summary>
    /// Asks the server to introduce a lag.
    /// </summary>
    /// <param name="time">Lag time.</param>
    /// <returns>Task completed when the request is sent.</returns>
    public async ValueTask IntroduceLagAsync(TimeSpan time)
        => await SendMessageAsync(new IntroduceLagRequest { Time = time });

    /// <summary>
    /// Asks the server to restart.
    /// </summary>
    /// <returns>Task completed when the request is sent.</returns>
    public async ValueTask RestartServerAsync()
        => await SendMessageAsync(new RestartRequest());

    /// <summary>
    /// Exchanges complex arrays.
    /// </summary>
    /// <returns>Task returning the array received.</returns>
    public async ValueTask<SingleItem[]> ComplexArrayAsync()
        => (await SendAndReceiveAsync<ComplexArray, ComplexArray>(new ComplexArray {
            Items = new[] {
                    new SingleItem { Id = Guid.NewGuid(), Number = 1, Text = "A", Array = Array.Empty<int>() },
                    new SingleItem { Id = Guid.NewGuid(), Number = 2, Text = "B", Array = Array.Empty<int>() },
            }
        })).Items;

}