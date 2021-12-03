namespace Woof.ConsoleTools;

/// <summary>
/// Defines console color theme assigning standard console colors to common text elements in hexadecimal format.
/// </summary>
public class ConsoleTheme {

    /// <summary>
    /// Gets or sets the default text color.
    /// </summary>
    public string DefaultText { get; set; } = "ccc";

    /// <summary>
    /// Gets or sets the header text color.
    /// </summary>
    public string HeaderText { get; set; } = "777";

    /// <summary>
    /// Gets or sets the bullet symbol color.
    /// </summary>
    public string BulletSymbol { get; set; } = "ff0";

    /// <summary>
    /// Gets or sets the bullet text color.
    /// </summary>
    public string BulletText { get; set; } = "ccc";

    /// <summary>
    /// Gets or sets the bullet symbol color for <see cref="ConsoleEx.Start(string)"/>.
    /// </summary>
    public string StartSymbol { get; set; } = "777";

    /// <summary>
    /// Gets or sets the bullet text color for <see cref="ConsoleEx.Start(string)"/>.
    /// </summary>
    public string StartText { get; set; } = "ccc";

    /// <summary>
    /// Gets or sets the color for complete label brackets.
    /// </summary>
    public string CompleteBrackets { get; set; } = "777";

    /// <summary>
    /// Gets or sets the color for complete label text with success status.
    /// </summary>
    public string CompleteLabelSuccess { get; set; } = "0f0";

    /// <summary>
    /// Gets or sets the color for complete label text with warning status.
    /// </summary>
    public string CompleteLabelWarning { get; set; } = "ff0";

    /// <summary>
    /// Gets or sets the color for complete label text with error status.
    /// </summary>
    public string CompleteLabelFail { get; set; } = "700";

    /// <summary>
    /// Gets or sets the color for complete message text.
    /// </summary>
    public string CompleteMessage { get; set; } = "777";

    /// <summary>
    /// Gets or sets the separator line color.
    /// </summary>
    public string SeparatorLine { get; set; } = "ccc";

    /// <summary>
    /// Gets or sets the assembly header title color.
    /// </summary>
    public string Title { get; set; } = "fff";

    /// <summary>
    /// Gets or sets the assembly header copyright color.
    /// </summary>
    public string Copyright { get; set; } = "fff";

    /// <summary>
    /// Gets or sets the assembly header description color.
    /// </summary>
    public string Description { get; set; } = "ccc";

    /// <summary>
    /// Gets or sets the timestamp text color.
    /// </summary>
    public string TimeStampText { get; set; } = "00f";

    /// <summary>
    /// Gets or sets the debug message text color.
    /// </summary>
    public string DebugText { get; set; } = "777";

    /// <summary>
    /// Gets or set the info label text color.
    /// </summary>
    public string InfoLabel { get; set; } = "fff";

    /// <summary>
    /// Gets or sets the warning label text color.
    /// </summary>
    public string WarningLabel { get; set; } = "770";

    /// <summary>
    /// Gets or sets the error label color.
    /// </summary>
    public string ErrorLabel { get; set; } = "700";

    /// <summary>
    /// Gets or sets the error value color.
    /// </summary>
    public string ErrorValue { get; set; } = "f00";

    /// <summary>
    /// Gets or sets the correct value color.
    /// </summary>
    public string CorrectValue { get; set; } = "0f0";

    /// <summary>
    /// Gets or sets the color for hexadecimal dump offset.
    /// </summary>
    public string HexOffset { get; set; } = "777";

    /// <summary>
    /// Gets or sets the color for hexadecimal dump first separator symbol.
    /// </summary>
    public string HexS1 { get; set; } = "0ff";

    /// <summary>
    /// Gets or sets the color for hexadecimal dump data.
    /// </summary>
    public string HexData { get; set; } = "077";

    /// <summary>
    /// Gets or sets the color for hexadecimal dump second separator symbol.
    /// </summary>
    public string HexS2 { get; set; } = "f00";

    /// <summary>
    /// Gets or sets the color for hexadecimal dump ASCII view.
    /// </summary>
    public string HexAscii { get; set; } = "777";

}
