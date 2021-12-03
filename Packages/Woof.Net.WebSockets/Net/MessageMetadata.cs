
using ProtoBuf;

namespace Woof.Net;

/// <summary>
/// Messge metadata header part.
/// </summary>
[ProtoContract]
public class MessageMetadata {

    /// <summary>
    /// Message type identifier.
    /// </summary>
    [ProtoMember(1)]
    public int TypeId { get; set; }

    /// <summary>
    /// Message identifier.
    /// </summary>
    [ProtoMember(2)]
    public Guid Id { get; set; }

    /// <summary>
    /// Message payload length.
    /// </summary>
    [ProtoMember(3)]
    public int PayloadLength { get; set; }

    /// <summary>
    /// Optional message signature.
    /// </summary>
    [ProtoMember(4)]
    public byte[]? Signature { get; set; }

}
