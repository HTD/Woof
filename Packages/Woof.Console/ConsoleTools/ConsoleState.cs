namespace Woof.ConsoleTools;

/// <summary>
/// Console state data.
/// </summary>
public struct ConsoleState {

    /// <summary>
    /// Console background color.
    /// </summary>
    public ConsoleColor Background;

    /// <summary>
    /// Console foreground color.
    /// </summary>
    public ConsoleColor Foreground;

    /// <summary>
    /// Cursor X coordinate.
    /// </summary>
    public int X;

    /// <summary>
    /// Cursor Y coordinate.
    /// </summary>
    public int Y;

    /// <summary>
    /// Window to buffer X coordinate.
    /// </summary>
    public int WinX;

    /// <summary>
    /// Window to buffer Y coorinate.
    /// </summary>
    public int WinY;

}
