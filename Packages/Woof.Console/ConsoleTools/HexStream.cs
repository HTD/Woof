namespace Woof.ConsoleTools;

/// <summary>
/// A stream to output hexadecimal dump to <see cref="Console"/>.
/// </summary>
public sealed partial class HexStream : Stream {

    /// <summary>
    /// This stream cannot be read.
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// This stream cannot be seeked.
    /// </summary>
    public override bool CanSeek => false;

    /// <summary>
    /// This stream can be written to.
    /// </summary>
    public override bool CanWrite => true;

    /// <summary>
    /// This stream does not return any length.
    /// </summary>
    public override long Length => -1;

    /// <summary>
    /// This stream doeasn't support setting position.
    /// </summary>
    public override long Position {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <summary>
    /// This stream ignores <see cref="Flush"/> method.
    /// </summary>
    public override void Flush() { }

    /// <summary>
    /// This stream cannot be read.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    /// <summary>
    /// This stream cannot be seeked.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    /// <summary>
    /// The lenght of this stream cannot be set.
    /// </summary>
    /// <param name="value"></param>
    public override void SetLength(long value) { }

    /// <summary>
    /// Outputs data as hexadecimal formatted dump.
    /// </summary>
    /// <param name="buffer">Buffer to dump.</param>
    /// <param name="offset">Optional offset (should be multiple of 16)</param>
    /// <param name="count">Number of bytes to dump.</param>
    public override void Write(byte[] buffer, int offset, int count) {
        if (buffer is null) return;
        var fs = Formats[ConsoleEx.IsHexColorEnabled];
        lock (ConsoleEx.Lock) {
            for (int i = 0; i < count; i++, offset++) {
                if (offset % 16 == 0) {
                    if (i >= 16) {
                        Console.Write(fs.TextSeparator);
                        Console.Write(fs.Text, TextDecode(buffer.AsSpan(offset - 16, 16)));
                    }
                    if (i > 0) Console.WriteLine();
                    Console.Write(fs.Offset, offset);
                }
                else if (offset % 4 == 0) {
                    Console.Write(fs.BlockSeparator);
                }
                Console.Write(fs.Byte, buffer[offset]);
            }
            var padding = 16 - count % 16;
            if (padding == 16) padding = 0;
            for (
                int i = 0; i <= padding; i++, offset++) {
                if (offset % 16 == 0) {
                    Console.Write(fs.TextSeparator);
                    Console.Write(fs.Text, TextDecode(buffer.AsSpan(offset - 16, 16 - padding)));
                    Console.WriteLine();
                    if (i < padding) Console.Write(fs.Offset, offset);
                }
                else if (offset % 4 == 0) {
                    Console.Write(fs.BlockSeparator);
                }
                if (i < padding) Console.Write(fs.Null);
            }
        }
    }

    /// <summary>
    /// Writes the entire buffer.
    /// </summary>
    /// <param name="buffer">Buffer.</param>
    public static void WriteData(byte[] buffer) {
        using var hs = new HexStream();
        hs.Write(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Writes the buffer fragmet.
    /// </summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="offset">First byte offset.</param>
    /// <param name="length">Fragment length.</param>
    public static void WriteData(byte[] buffer, int offset, int length) {
        using var hs = new HexStream();
        hs.Write(buffer, offset, length);
    }

    /// <summary>
    /// Writes the buffer span.
    /// </summary>
    /// <param name="buffer">Memory region.</param>
    public static void WriteData(ReadOnlyMemory<byte> buffer) {
        using var hs = new HexStream();
        hs.Write(buffer.Span.ToArray(), 0, buffer.Length);
    }

    /// <summary>
    /// Writes the entire stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    public static void WriteData(Stream stream) {
        using var hs = new HexStream();
        stream.CopyTo(hs);
    }

    /// <summary>
    /// Writes a specified portion of the stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="offset">Stream offset to seek.</param>
    /// <param name="length">Fragment length.</param>
    public static void WriteData(Stream stream, int offset, int length) {
        using var d = new HexStream();
        var buffer = new byte[length];
        stream.Position = offset;
        stream.Read(buffer, 0, length);
        d.Write(buffer, 0, length);
    }

    /// <summary>
    /// Decodes the text from bytes.
    /// </summary>
    /// <param name="buffer">Data buffer.</param>
    /// <returns>Decoded string.</returns>
    private static string TextDecode(Span<byte> buffer) {
        var result = Console.OutputEncoding.GetString(buffer);
        var filtered = "";
        for (int i = 0, n = result.Length; i < n; i++) {
            filtered += (result[i] < 0x20) ? '.' : result[i];
        }
        return ConsoleEx.IsHexColorEnabled ? filtered.Replace("`", "``") : filtered;
    }

}