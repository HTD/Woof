namespace Woof.ConsoleTools;

/// <summary>
/// A special object allowing to display dots by one thread, while the other already displayed something else on the console.
/// </summary>
public sealed class Cursor {

    /// <summary>
    /// Gets this current cursor position.
    /// </summary>
    public (int X, int Y) Position => (L.X, L.Y);

    /// <summary>
    /// Creates progress dots placeholder at current cursor position.
    /// </summary>
    public Cursor() {

        lock (ConsoleEx.Lock) {
            L = C;
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays subsequent progress dot.
    /// </summary>
    public void Dot() {
        lock (ConsoleEx.Lock) {
            B = C;
            C = L;
            Console.Write('.');
            L = C;
            C = B;
        }
    }

    /// <summary>
    /// Outputs a text to the console at the curent <see cref="Cursor"/> instance position.
    /// </summary>
    /// <param name="text">Text to output.</param>
    public void Write(string text) {
        lock (ConsoleEx.Lock) {
            B = C;
            C = L;
            Console.Write(text);
            L = C;
            C = B;
            if (ConsoleEx.IsHexColorEnabled) Console.ResetColor();
        }
    }

    /// <summary>
    /// Outputs a text to the console at the <paramref name="x"/> position and curent <see cref="Cursor"/> instance Y position.<br/>
    /// Cursor position is not changed.
    /// </summary>
    /// <param name="text">Text to output.</param>
    /// <param name="x">Column index.</param>
    public void Write(string text, int x) {
        lock (ConsoleEx.Lock) {
            B = C;
            C = L;
            Console.SetCursorPosition(x, C.Y);
            Console.WriteLine(text);
            C = B;
            if (ConsoleEx.IsHexColorEnabled) Console.ResetColor();
        }
    }

    /// <summary>
    /// Outputs a text to the console at the curent <see cref="Cursor"/> instance position and moves the cursor to the new line.
    /// </summary>
    /// <param name="text">Text to output.</param>
    public void WriteLine(string text) {
        lock (ConsoleEx.Lock) {
            B = C;
            C = L;
            Console.WriteLine(text);
            L = C;
            C = B;
            if (ConsoleEx.IsHexColorEnabled) Console.ResetColor();
        }
    }

    #region Private data

    /// <summary>
    /// Backup of console state.
    /// </summary>
    ConsoleState B;

    /// <summary>
    /// Last console state.
    /// </summary>
    private ConsoleState L;

    /// <summary>
    /// Current console state.
    /// </summary>
    static ConsoleState C {
        get => ConsoleEx.State; set => ConsoleEx.State = value;
    }

    #endregion

}