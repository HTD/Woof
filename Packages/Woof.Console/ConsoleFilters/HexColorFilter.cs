namespace Woof.ConsoleFilters;

/// <summary>
/// Text filter that understands hexadecimal color codes and sets console colors to their closest match.
/// <para>USAGE: Console.SetOut(new HexColorFilter());</para>
/// </summary>
/// <remarks>
/// <para>
/// First backtick character starts hex color mode. Next backtick ends it.
/// Backtick not followed by hexadecimal digit resets default colors.
/// </para>
/// <para>
/// Hex colors consist of 3 hexadecimal digits (are 12-bit hexadecimal numbers).
/// Exact color values are normalized to match preset console colors.
/// </para>
/// <para>
/// If 4-th special charcter is present ('b' or '_'), the color is interpreted as background color.
/// </para>
/// <para>Example:</para>
/// <para>"Hello `070`green` world!" - outputs the word "green"... green.</para>
/// <para>"Hello `000``700b`red` world!" - outputs the word "red"... black on red background.</para>
/// </remarks>
public class HexColorFilter : TextWriter {

    /// <summary>
    /// Creates the filter over text output.
    /// </summary>
    public HexColorFilter() => Out = Console.Out;

    /// <summary>
    /// Console normalized colors table.
    /// </summary>
    readonly Dictionary<int, ConsoleColor> Colors = new() {
        [0x000] = ConsoleColor.Black,
        [0x007] = ConsoleColor.DarkBlue,
        [0x070] = ConsoleColor.DarkGreen,
        [0x077] = ConsoleColor.DarkCyan,
        [0x700] = ConsoleColor.DarkRed,
        [0x707] = ConsoleColor.DarkMagenta,
        [0x770] = ConsoleColor.DarkYellow,
        [0x777] = ConsoleColor.DarkGray,
        [0xccc] = ConsoleColor.Gray,
        [0x00f] = ConsoleColor.Blue,
        [0x0f0] = ConsoleColor.Green,
        [0x0ff] = ConsoleColor.Cyan,
        [0xf00] = ConsoleColor.Red,
        [0xf0f] = ConsoleColor.Magenta,
        [0xff0] = ConsoleColor.Yellow,
        [0xfff] = ConsoleColor.White
    };

    /// <summary>
    /// Gets bound console encoding.
    /// </summary>
    public override Encoding Encoding => Console.OutputEncoding;

    /// <summary>
    /// Writes a character to the console or tries parse it.
    /// </summary>
    /// <param name="c"></param>
    public override void Write(char c) {
        if (!IsHexColorMode) {
            if (c == '`') IsHexColorMode = true;
            else Out.Write(c);
        }
        else {
            if (c == '`') ParseCode();
            //if (HexCode != null && HexCode.Length < 1) Reset(c, false);
            else if (!IsHex(c) && !IsBgc(c)) Reset(c);
            else HexCode += c;
        }
    }

    /// <summary>
    /// Parses matched text as hex code.
    /// If code matches the console colors are set accordingly.
    /// </summary>
    private void ParseCode() {
        int l = HexCode.Length;
        if (l == 0) { IsHexColorMode = false; Out.Write('`'); return; }
        char c;
        int r, g, b, i;
        bool isBackground = false;
        if (l >= 3) {
            c = HexCode[0]; if (!IsHex(c)) return;
            r = Normalize(Convert.ToInt32(c.ToString(), 16));
            c = HexCode[1]; if (!IsHex(c)) return;
            g = Normalize(Convert.ToInt32(c.ToString(), 16));
            c = HexCode[2]; if (!IsHex(c)) return;
            b = Normalize(Convert.ToInt32(c.ToString(), 16));
            if (l > 3) {
                c = HexCode[3];
                isBackground = IsBgc(c);
            }
            i = r << 8 | g << 4 | b;
            if (Colors.ContainsKey(i)) {
                if (!isBackground) Console.ForegroundColor = Colors[i];
                else Console.BackgroundColor = Colors[i];
            }
        }
        Exit();
    }

    /// <summary>
    /// Resets console colors and exits hex code mode.
    /// </summary>
    /// <param name="c">Optional character to output.</param>
    private void Reset(char? c = null) {
        Console.ResetColor();
        if (c.HasValue) Out.Write(c);
        Exit();
    }

    /// <summary>
    /// Exits hex code mode.
    /// </summary>
    private void Exit() {
        HexCode = String.Empty;
        IsHexColorMode = false;
    }

    /// <summary>
    /// Tests a character for being a hexadecimal digit.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns>True if hexadecimal digit.</returns>
    static bool IsHex(char c) => c is >= '0' and <= '9' or >= 'A' and < 'F' or >= 'a' and <= 'f';

    /// <summary>
    /// Tests a character for being background color flag.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    static bool IsBgc(char c) => c is 'b' or '_';

    /// <summary>
    /// Normalizes 4-bit number to make it one of 4 possible values.
    /// </summary>
    /// <param name="n">Number.</param>
    /// <returns>Normalized number.</returns>
    static int Normalize(int n) => n switch {
        < 0x4 => 0x0,
        < 0x8 => 0x7,
        < 0xd => 0xc,
        _ => 0xf
    };

    /// <summary>
    /// Hex color code current buffer.
    /// </summary>
    string HexCode = String.Empty;

    /// <summary>
    /// True if the filter is currently in hex color mode.
    /// </summary>
    bool IsHexColorMode;

    /// <summary>
    /// Console out text writer.
    /// </summary>
    public readonly TextWriter Out;

}