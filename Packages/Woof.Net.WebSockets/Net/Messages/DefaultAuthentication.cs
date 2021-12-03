using ProtoBuf;

namespace Woof.Net.Messages;

/// <summary>
/// Client sign in request.
/// </summary>
[Message(0x1000_1000, IsSigned = true), ProtoContract]
public class SignInRequest : ISignInRequest { // this special kind of request MUST implement ISignRequest.

    /// <summary>
    /// Gets or sets the API key in binary form. Reqiured by <see cref="ISignInRequest"/> interface.
    /// </summary>
    [ProtoMember(1)]
    public byte[] ApiKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets client's globally unique identifier.
    /// </summary>
    [ProtoMember(2)]
    public Guid ClientId { get; set; }

}

/// <summary>
/// Client sign in response. Sent if the client authentication succeeded.
/// </summary>
[Message(0x1000_1001), ProtoContract]
public class SignInResponse { }

/// <summary>
/// Client sign out request. Dosen't require response.
/// </summary>
[Message(0x1000_1010), ProtoContract]
public class SignOutRequest { }
