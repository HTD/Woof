using ProtoBuf;

namespace Woof.Net.Messages;

/// <summary>
/// The server could not understand the request due to invalid format.
/// </summary>
[Message(400), ProtoContract]
public class E400_BadRequestResponse { }

/// <summary>
/// The client must authenticate itself to get the requested response.
/// </summary>
[Message(401), ProtoContract]
public class E401_UnauthorizedResponse { }

/// <summary>
/// The client does not have access rights to the content.
/// </summary>
[Message(403), ProtoContract]
public class E403_ForbiddenResponse { }

/// <summary>
/// The server can not find the requested resource.
/// </summary>
[Message(404), ProtoContract]
public class E404_NotFoundResponse { }

/// <summary>
/// The server has encountered a situation it doesn't know how to handle.
/// </summary>
[Message(500), ProtoContract]
public class E500_InternalServerErrorResponse : IGenericErrorResponse {

    /// <summary>
    /// Gets or sets an error code agreed between client and server to identify the error type. Zero for not-specified.
    /// </summary>
    [ProtoMember(1)]
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
    /// </summary>
    [ProtoMember(2)]
    public int HResult { get; set; }

    /// <summary>
    /// Gets or sets the optional error message / description.
    /// </summary>
    [ProtoMember(3)]
    public string? Description { get; set; }

}

/// <summary>
/// The server has encountered a situation it doesn't know how to handle. Contains diagnostic information.
/// </summary>
[Message(0x1000_0000 + 500), ProtoContract]
public class E500_InternalServerErrorDiagnosticResponse : IDiagnosticErrorResponse {

    /// <summary>
    /// Gets or sets an error code agreed between client and server to identify the error type. Zero for not-specified.
    /// </summary>
    [ProtoMember(1)]
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets the optional error message / description.
    /// </summary>
    [ProtoMember(2)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
    /// </summary>
    [ProtoMember(3)]
    public int HResult { get; set; }

    /// <summary>
    /// Gets a string representation of the immediate frames on the <see cref="Exception"/> call stack.
    /// </summary>
    [ProtoMember(4)]
    public string? StackTrace { get; set; }

}

/// <summary>
/// Client authentication failed.
/// </summary>
[Message(0x1000_0001), ProtoContract]
public class AuthenticationErrorResponse : IGenericErrorResponse {

    /// <summary>
    /// Gets or sets authentication error code: 1 - API key rejected, 2 - ClientId rejected, 3 - both key and ClientId rejected.
    /// </summary>
    [ProtoMember(1)]
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets the optional description of the authentication error that occured.
    /// </summary>
    [ProtoMember(2)]
    public string? Description { get; set; }

}