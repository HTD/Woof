using System;

using ProtoBuf;

using Woof.Net;

namespace Test.Api;

[Message(0x3000), ProtoContract]
public class TimeSubscribeRequest {

    [ProtoMember(1)]
    public TimeSpan Period { get; set; }

}

[Message(0x4000), ProtoContract]
public class TimeNotification {

    [ProtoMember(1)]
    public DateTime Time { get; set; }

}

[Message(0x5000), ProtoContract]
public class DivideRequest {

    /// <summary>
    /// Note that the decimal type is just to make it harder for the serializer.
    /// </summary>
    [ProtoMember(1)]
    public decimal X { get; set; }

    [ProtoMember(2)]
    public decimal Y { get; set; }

}

[Message(0x5001), ProtoContract]
public class DivideResponse {

    [ProtoMember(1)]
    public decimal Result { get; set; }

}

[Message(0x5010, IsSigned = true), ProtoContract]
public class PrivateRequest {

    [ProtoMember(1)]
    public DateTime ClientTime { get; set; }

}

[Message(0x5011), ProtoContract]
public class PrivateResponse {

    [ProtoMember(1)]
    public string Secret { get; set; }

}

[Message(0x6000), ProtoContract]
public class ComplexArray {

    [ProtoMember(1)]
    public SingleItem[] Items { get; set; }

}

[ProtoContract]
public class SingleItem {

    [ProtoMember(1)]
    public Guid Id { get; set; }

    [ProtoMember(2)]
    public int Number { get; set; }

    [ProtoMember(3)]
    public string Text { get; set; }

    [ProtoMember(4)]
    public int[] Array { get; set; }

}

[Message(0x8000), ProtoContract]
public class RawMessageRequest {

    [ProtoMember(1)]
    public int TypeId { get; set; } = 0x1000_0000;

    [ProtoMember(2)]
    public byte[] Data { get; set; }

}

[Message(0x8001), ProtoContract]
public class RawMessageResponse {

    [ProtoMember(1)]
    public int TypeId { get; set; }

    [ProtoMember(2)]
    public byte[] Data { get; set; }

}

[Message(0x8010), ProtoContract]
public class IgnoreMessagesRequest {

    [ProtoMember(1)]
    public int Number { get; set; }

}

[Message(0x8020), ProtoContract]
public class IntroduceLagRequest {

    [ProtoMember(1)]
    public TimeSpan Time { get; set; }

}

[Message(0x8030), ProtoContract]
public class StrictSignInPolicyRequest {

    [ProtoMember(1)]
    public bool DisconnectOnAuthenticationFailed { get; set; }

}

[Message(0x8100), ProtoContract]
public class RestartRequest { }