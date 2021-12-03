namespace Woof.ConsoleTools;

public sealed partial class HexStream {

    /// <summary>
    /// Contains string formats for various <see cref="HexStream"/> parts.
    /// </summary>
    public class FormatStrings {

        /// <summary>
        /// A string format for offset values.
        /// </summary>
        public string Offset { get; set; } = String.Empty;

        /// <summary>
        /// A string format for a single byte.
        /// </summary>
        public string Byte { get; set; } = String.Empty;

        /// <summary>
        /// A string format for block separators.
        /// </summary>
        public string BlockSeparator { get; set; } = String.Empty;

        /// <summary>
        /// A string format for text separators.
        /// </summary>
        public string TextSeparator { get; set; } = String.Empty;

        /// <summary>
        /// A string format for text dump.
        /// </summary>
        public string Text { get; set; } = String.Empty;

        /// <summary>
        /// A string format for null (empty) bytes.
        /// </summary>
        public string Null { get; set; } = String.Empty;

    }

    /// <summary>
    /// Output formats definitions.
    /// </summary>
    private readonly Dictionary<bool, FormatStrings> Formats = new() {
        [false] = new() { // monochrome output
            Offset = "{0:x8} : ",
            Byte = "{0:x2} ",
            BlockSeparator = " ",
            TextSeparator = ": ",
            Text = "{0}",
            Null = "   "
        },
        [true] = new() { // color output
            Offset = $"`{Theme.HexOffset}`" + "{0:x8} " + $"`{Theme.HexS1}`" + ":` ",
            Byte = $"`{Theme.HexData}`" + "{0:x2}` ",
            BlockSeparator = " ",
            TextSeparator = $"`{Theme.HexS2}`:` ",
            Text = $"`{Theme.HexAscii}`" + "{0}`",
            Null = "   "
        }
    };

    private static ConsoleTheme Theme => ConsoleEx.Theme;

}