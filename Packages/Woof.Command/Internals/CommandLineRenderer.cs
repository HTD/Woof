using System.Security;

namespace Woof.Command.Internals;

/// <summary>
/// Optionally interactive command line processor / console renderer.
/// </summary>
internal class CommandLineRenderer : CommandLine {

    #region Properties

    /// <summary>
    /// Gets or sets the display console color of the command line arguments.
    /// </summary>
    public ConsoleColor ArgumentsColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the display console color of the command line command element.
    /// </summary>
    public ConsoleColor CommandColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Gets the index of the part pointed with the current cursor position.
    /// </summary>
    public int CurrentPartIndex { get; private set; }

    /// <summary>
    /// Gets the line offset of the part pointed with the current cursor position.
    /// </summary>
    public int CurrentPartOffset { get; private set; }

    /// <summary>
    /// Gets the length of the part pointed with the current cursor position.
    /// </summary>
    public int CurrentPartLength { get; private set; }

    /// <summary>
    /// Gets or sets (replaces) the unquoted part pointed with the current cursor position.
    /// </summary>
    public string? CurrentPart {
        get => (CurrentPartIndex >= 0 && CurrentPartOffset >= 0 && CurrentPartLength > 0) ? Text.Substring(CurrentPartOffset, CurrentPartLength) : null;
        set {
            if (value == null) return;
            if (CurrentPartIndex >= 0 && CurrentPartOffset >= 0 && CurrentPartLength > 0) {
                var quoted = Quote(value);
                int offset = CurrentPartOffset, length = quoted.Length;
                Text = Text.Remove(CurrentPartOffset, CurrentPartLength).Insert(CurrentPartOffset, quoted);
                Cursor = offset + length;
            }
        }
    }

    /// <summary>
    /// Gets or sets a position of the cursor that is used to point the parts within the command line.
    /// </summary>
    public int Cursor {
        get => _Cursor;
        set {
            _Cursor = value;
            CurrentPartIndex = -1;
            CurrentPartOffset = -1;
            CurrentPartLength = -1;
            if (_Cursor < 0 || _Cursor > Text.Length || Text.Length < 1) return;
            var mapIndex = _Cursor < Text.Length ? _Cursor : Text.Length - 1;
            CurrentPartIndex = Map[mapIndex];
            CurrentPartLength = 0;
            for (int i = 0, n = Map.Length; i < n; i++) {
                if (Map[i] == CurrentPartIndex) {
                    if (CurrentPartOffset < 0) { CurrentPartOffset = i; }
                    CurrentPartLength++;
                }
            }
            while (CurrentPartOffset + CurrentPartLength > Text.Length) CurrentPartLength--;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the line input should work in overtype mode.
    /// </summary>
    public bool IsOvertype {
        get => _IsOvertype;
        set {
            _IsOvertype = value;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Console.CursorSize = value ? 100 : 10;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this command line is rendered to the console.
    /// </summary>
    public bool IsRendered { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new empty command line.
    /// </summary>
    public CommandLineRenderer() { }

    /// <summary>
    /// Creates a new command line from string.
    /// </summary>
    /// <param name="line">Source command line.</param>
    public CommandLineRenderer(string line) => Text = line;

    #endregion

    #region Cursor position

    /// <summary>
    /// Moves the cursor to the beginning of the command line.
    /// </summary>
    /// <param name="update">True if the console cursor should be updated with the virtual cursor.</param>
    public void Home(bool update = true) {
        Cursor = 0;
        if (update) UpdateCursor();
    }

    /// <summary>
    /// Moves the cursor to previous part or beginning of the current part.
    /// </summary>
    /// <param name="update">True if the console cursor should be updated with the virtual cursor.</param>
    public void Prev(bool update = true) {
        if (Cursor > 0) {
            int i = _Cursor;
            if (i > 0 && Text[i - 1] == ' ') Cursor = --i;
            for (; i >= 0; i--) if (GetPartIndex(i) != CurrentPartIndex) break;
            Cursor = ++i;
            if (update) UpdateCursor();
        }
    }

    /// <summary>
    /// Moves the cursor one character left if applicable.
    /// </summary>
    /// <param name="update">True if the console cursor should be updated with the virtual cursor.</param>
    public void Left(bool update = true) {
        if (Cursor > 0) {
            Cursor--;
            if (update) UpdateCursor();
        }
    }

    /// <summary>
    /// Moves the cursor one character right if applicable.
    /// </summary>
    /// <param name="update">True if the console cursor should be updated with the virtual cursor.</param>
    public void Right(bool update = true) {
        if (Cursor < Text.Length) {
            Cursor++;
            if (update) UpdateCursor();
        }
    }

    /// <summary>
    /// Moves the cursor to the next part or the line end.
    /// </summary>
    /// <param name="update">True if the console cursor should be updated with the virtual cursor.</param>
    public void Next(bool update = true) {
        if (Cursor < Text.Length) {
            int i = _Cursor;
            if (i < Text.Length && Text[i] == ' ') Cursor = ++i;
            for (; i <= Text.Length; i++) if (GetPartIndex(i) != CurrentPartIndex) break;
            Cursor = --i;
            if (update) UpdateCursor();
        }
    }

    /// <summary>
    /// Moves the cursor to the end of the command line.
    /// </summary>
    /// <param name="update">True if the console cursor should be updated with the virtual cursor.</param>
    public void End(bool update = true) {
        Cursor = Text.Length;
        if (update) UpdateCursor();
    }

    /// <summary>
    /// Resets the X coordinate offset for the home cursor position.
    /// </summary>
    public void SetCursorHome() => _XOffset = Console.CursorLeft;

    #endregion

    #region Console interactive renderer

    /// <summary>
    /// Renders the current command line to the console.
    /// </summary>
    public void Render() {
        if (!IsRendered) {
            Console.CursorVisible = false;
            _XOffset = Console.CursorLeft;
            if (_Cursor < 0) Cursor = Text.Length;
            IsRendered = true;
        }
        if (String.IsNullOrEmpty(Text)) {
            UpdateCursor(true);
            _LastLength = 0;
            return;
        }
        int i, j = 1, n = Map.Length;
        for (i = 0; i < n; i++) {
            if (j == 0 && Map[i] != 0) break;
            if (j != 0 && Map[i] == 0) j--;
        }
        var foregroundDefault = Console.ForegroundColor;
        Console.CursorVisible = false;
        Console.ForegroundColor = CommandColor;
        Console.CursorLeft = _XOffset;
        Console.Write(Text[..i]);
        Console.ForegroundColor = ArgumentsColor;
        Console.Write(Text[i..]);
        Console.ForegroundColor = foregroundDefault;
        Console.CursorLeft = _XOffset + Cursor;
        Console.CursorVisible = true;
        _LastLength = Text.Length;
    }

    /// <summary>
    /// Updates the text of the command line.
    /// </summary>
    public void UpdateText() {
        if (!IsRendered) return;
        var l0 = _LastLength;
        Console.CursorVisible = false;
        Render();
        var dl = l0 - Text.Length;
        if (dl > 0) {
            var lastLeft = Console.CursorLeft;
            Console.CursorLeft = _XOffset + Text.Length;
            Console.Write("".PadRight(dl));
            Console.CursorLeft = lastLeft;
        }
        if (Console.CursorLeft > _XOffset + Text.Length) Console.CursorLeft = _XOffset + Text.Length;
        Cursor = _Cursor;
        Console.CursorVisible = true;
    }

    /// <summary>
    /// Updates the console cursor position and optionally - visibility.
    /// </summary>
    /// <param name="visible">If not null, the cursor visibility will be changed to this value.</param>
    public void UpdateCursor(bool? visible = null) {
        if (!IsRendered) return;
        if (visible == false) Console.CursorVisible = false;
        Console.CursorLeft = _XOffset + Cursor;
        if (visible == true) Console.CursorVisible = true;
    }

    /// <summary>
    /// Types one character into this command line.
    /// </summary>
    /// <param name="c">Character to type.</param>
    public void Type(char c) {
        if (IsOvertype) {
            if (_Cursor == Text.Length) Text += c;
            else Text = Text[.._Cursor] + c + Text[(_Cursor + 1)..];
        }
        else {
            Text = Text.Insert(_Cursor, c.ToString());
        }
        Cursor++;
        UpdateText();
    }

    /// <summary>
    /// Types a sequence of characters into this command line.
    /// </summary>
    /// <param name="s">A string of characters to type.</param>
    public void Type(string s) {
        if (IsOvertype) {
            if (_Cursor == Text.Length) Text += s;
            else Text = Text[.._Cursor] + s + ((Cursor + s.Length <= Text.Length) ? Text[(_Cursor + s.Length)..] : "");
        }
        else {
            Text = Text.Insert(_Cursor, s);
        }
        Cursor += s.Length;
        UpdateText();
    }

    /// <summary>
    /// Deletes one character back from the cursor position. Moves the cursor 1 character left.
    /// </summary>
    public void Backspace() {
        if (Cursor > 0) {
            Text = Text.Remove(--Cursor, 1);
            UpdateText();
        }
    }

    /// <summary>
    /// Deletes one character on the cursor position.
    /// </summary>
    public void Delete() {
        if (Cursor < Text.Length) {
            Text = Text.Remove(Cursor, 1);
            UpdateText();
        }
    }

    /// <summary>
    /// Accepts a single key and performs an operation on this command line if applicable.
    /// </summary>
    /// <param name="k">Key that was pressed.</param>
    /// <returns>True if the key was handled by the command line itself.</returns>
    public bool AcceptKey(ConsoleKeyInfo k) {
        if (k.Modifiers == 0) {
            switch (k.Key) {
                case ConsoleKey.Backspace: Backspace(); return true;
                case ConsoleKey.Delete: Delete(); return true;
                case ConsoleKey.Home: Home(); return true;
                case ConsoleKey.LeftArrow: Left(); return true;
                case ConsoleKey.RightArrow: Right(); return true;
                case ConsoleKey.End: End(); return true;
                case ConsoleKey.Insert: IsOvertype = !IsOvertype; return true;
            }
        }
        if (k.Modifiers.HasFlag(ConsoleModifiers.Control) && !k.Modifiers.HasFlag(ConsoleModifiers.Alt)) {
            switch (k.Key) {
                case ConsoleKey.LeftArrow: Prev(); return true;
                case ConsoleKey.RightArrow: Next(); return true;
            }
        }
        if ((!k.Modifiers.HasFlag(ConsoleModifiers.Control) || k.Modifiers.HasFlag(ConsoleModifiers.Alt)) && !_NonPrintableKeys.Contains(k.Key)) {
            Type(k.KeyChar);
            return true;
        }
        return false;
    }

    #endregion

    #region Static methods

    /// <summary>
    /// Reads a password from the console in a secure way.
    /// </summary>
    /// <returns>Password as an unmanaged <see cref="SecureString"/>.</returns>
    public static SecureString? ReadPassword() {
        var passwd = new SecureString();
        ConsoleKeyInfo k;
        while ((k = Console.ReadKey(true)).Key != ConsoleKey.Enter) {
            if (k.Key == ConsoleKey.Escape) { Console.WriteLine(); return null; }
            else if (k.Key == ConsoleKey.Backspace) {
                if (passwd.Length > 0) {
                    passwd.RemoveAt(passwd.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (k.Modifiers.HasFlag(ConsoleModifiers.Control) && !k.Modifiers.HasFlag(ConsoleModifiers.Alt) || _NonPrintableKeys.Contains(k.Key)) continue;
            else {
                if (k.Modifiers is not ConsoleModifiers.Control and not (ConsoleModifiers.Control | ConsoleModifiers.Shift)) {
                    passwd.AppendChar(k.KeyChar);
                    Console.Write("*");
                }
            }
        }
        Console.WriteLine();
        return passwd;
    }

    #endregion

    #region Private code

    /// <summary>
    /// Gets the part index at specified position within the command line text.
    /// </summary>
    /// <param name="at">Position within the command line text.</param>
    /// <returns>Part index, negative for whitespace that doesn't belong to any part.</returns>
    private int GetPartIndex(int at) => at < 0 ? 0 : Map[at < Text.Length ? at : Text.Length - 1];

    #endregion

    #region Private data

    /// <summary>
    /// Cursor position cache.
    /// </summary>
    private int _Cursor;

    /// <summary>
    /// <see cref="IsOvertype"/> cache.
    /// </summary>
    private bool _IsOvertype;

    /// <summary>
    /// The length of the command line in last render operation.
    /// </summary>
    private int _LastLength;

    /// <summary>
    /// Keys to be ignored by <see cref="Type(char)"/> method.
    /// </summary>
    private static readonly ConsoleKey[] _NonPrintableKeys = [
            ConsoleKey.Backspace, ConsoleKey.Tab, ConsoleKey.Enter,
            ConsoleKey.Pause, ConsoleKey.Escape, ConsoleKey.PageUp, ConsoleKey.PageDown, ConsoleKey.End, ConsoleKey.Home,
            ConsoleKey.LeftArrow, ConsoleKey.UpArrow, ConsoleKey.RightArrow, ConsoleKey.DownArrow,
            ConsoleKey.PrintScreen, ConsoleKey.Insert, ConsoleKey.Delete,
            ConsoleKey.LeftWindows, ConsoleKey.RightWindows,
            ConsoleKey.F1, ConsoleKey.F2, ConsoleKey.F3, ConsoleKey.F4, ConsoleKey.F5, ConsoleKey.F6, ConsoleKey.F7, ConsoleKey.F8,
            ConsoleKey.F9, ConsoleKey.F10, ConsoleKey.F11, ConsoleKey.F12, ConsoleKey.F13, ConsoleKey.F14, ConsoleKey.F15, ConsoleKey.F16,
            ConsoleKey.F17, ConsoleKey.F18, ConsoleKey.F19, ConsoleKey.F20, ConsoleKey.F21, ConsoleKey.F22, ConsoleKey.F23, ConsoleKey.F24
        ];

    /// <summary>
    /// X coordinate of the original console position of the rendered command line.
    /// </summary>
    private int _XOffset;

    #endregion

}