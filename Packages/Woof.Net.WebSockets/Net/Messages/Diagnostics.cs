using ProtoBuf;

namespace Woof.Net.Messages;

/// <summary>
/// Ping request, used to test connectivity and response time.
/// </summary>
[Message(0x1000_5000), ProtoContract]
public class PingRequest { }

/// <summary>
/// Ping response, used to test connectivity and response time.
/// </summary>
[Message(0x1000_5001), ProtoContract]
public class PingResponse { }

/// <summary>
/// Identification request, used to identify the endpoint.
/// </summary>
[Message(0x1000_6000), ProtoContract]
public class IdRequest { }

/// <summary>
/// Identification response, used to identify the endpoint.
/// </summary>
[Message(0x1000_6001), ProtoContract]
public class IdResponse {

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    [ProtoMember(1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the server version.
    /// </summary>
    [ProtoMember(2)]
    public string Version { get; set; } = null!;

    /// <summary>
    /// Gets or sets the server build date.
    /// </summary>
    [ProtoMember(3)]
    public DateTime BuildTime { get; set; }

    /// <summary>
    /// Gets or sets the server up time.
    /// </summary>
    [ProtoMember(4)]
    public TimeSpan UpTime { get; set; }

    /// <summary>
    /// Gets or sets the server timeout setting.
    /// </summary>
    [ProtoMember(5)]
    public TimeSpan Timeout { get; set; }

}

/// <summary>
/// Query API request, used to query available API messages.
/// To get private API messages the user must be authenticated.
/// </summary>
[Message(0x1000_6010), ProtoContract]
public class QueryApiRequest { 

    /// <summary>
    /// Gets or sets a value indicating that no internal API members should be returned.
    /// </summary>
    [ProtoMember(1)]
    public bool NoInternals { get; set; }

}

/// <summary>
/// Query API response, used to fetch available API messages.
/// </summary>
[Message(0x1000_6011), ProtoContract]
public class QueryApiResponse {

    /// <summary>
    /// Gets or sets the API messages understood by the end point.
    /// </summary>
    [ProtoMember(1)]
    public Dictionary<int, string> Messages { get; set; } = [];

}

/// <summary>
/// A request testing exception handling. This request should raise an <see cref="InvalidOperationException"/>.
/// </summary>
[Message(0x1000_6020), ProtoContract]
public class TestExceptionRequest { }

/// <summary>
/// An invalid response for a valid <see cref="TestExceptionRequest"/>.
/// This is what should NOT be sent.
/// </summary>
[Message(0x1000_6021), ProtoContract]
public class TestExceptionResponse { }

/// <summary>
/// Request to fetch a stream fragment from the other endpoint.
/// </summary>
[Message(0x1000_6100), ProtoContract]
public sealed class GetStreamFragmentRequest {

    /// <summary>
    /// Gets or sets the globally unique stream identifier. The field identifies the target stream.
    /// </summary>
    [ProtoMember(1)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the position the stream should be read from.<br/>
    /// Positive numbers including zero represent the position value from the beginning of the stream.<br/>
    /// Any negative number means that the position will be ignored
    /// and each subsequent read will fetch the stream from the current internal stream position.<br/>
    /// DO NOT USE when more than one endpoint can access the same stream.
    /// </summary>
    [ProtoMember(2)]
    public long Position { get; set; }

    /// <summary>
    /// Gets or sets the value indicating the requested data length in bytes.
    /// </summary>
    [ProtoMember(3)]
    public int Length { get; set; }

}

/// <summary>
/// A response to the <see cref="GetStreamFragmentRequest"/>.
/// </summary>
[Message(0x1000_6101), ProtoContract]
public sealed class GetStreamFragmentResponse {

    /// <summary>
    /// Gets or sets a value indicating the message contains the end of the data.
    /// </summary>
    [ProtoMember(1)]
    public bool IsEnd { get; set; }

    /// <summary>
    /// Gets or sets the message data buffer.
    /// The buffer can contain only the data with no padding.
    /// The buffer length represents the data length.
    /// </summary>
    [ProtoMember(2)]
    public byte[] Buffer { get; set; } = Array.Empty<byte>();

}