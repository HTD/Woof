namespace Woof.Net.Messages;

/// <summary>
/// Defines a diagnostic error response containing diagnostic data.
/// </summary>
public interface IDiagnosticErrorResponse : IGenericErrorResponse {

    /// <summary>
    /// Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
    /// </summary>
    public int HResult { get; set; }

    /// <summary>
    /// Gets a string representation of the immediate frames on the call stack.
    /// </summary>
    public string? StackTrace { get; set; }

}
