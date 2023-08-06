namespace Woof.Settings;

/// <summary>
/// Decodes the Unicode escape sequences while writing UTF-8 stream.
/// </summary>
/// <remarks>
/// This is a workaround for a <see cref="Utf8JsonWriter"/> not doing it on its own.
/// </remarks>
public class Utf8DecodeStream : Stream {

    /// <summary>
    /// Creates a Unicode escape sequence decoding stream over a writeable stream.
    /// </summary>
    /// <param name="stream">A writeable stream.</param>
    public Utf8DecodeStream(Stream stream) => InnerStream = stream;

#pragma warning disable CS1591

    public override bool CanRead => InnerStream.CanRead;

    public override bool CanSeek => InnerStream.CanSeek;

    public override bool CanWrite => InnerStream.CanWrite;

    public override long Length => InnerStream.Length;

    public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

    public override void Flush() => InnerStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => InnerStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => InnerStream.Seek(offset, origin);

    public override void SetLength(long value) => InnerStream.SetLength(value);

#pragma warning restore CS1591

    /// <summary>
    /// Writes the buffer with the Unicode sequences decoded.
    /// </summary>
    /// <param name="buffer">Buffer to write.</param>
    /// <param name="offset">Position in the buffer to start.</param>
    /// <param name="count">Number of bytes to write.</param>
    public override void Write(byte[] buffer, int offset, int count) {
        while (count > 0) {
            bool sequenceFound = false;
            for (int i = offset, n = offset + count; i < n; i++) {
                if (DecodeUtf8Sequence(buffer, i, out var sequence, out var bytesConsumed)) {
                    InnerStream.Write(buffer, offset, i - offset);
                    count -= i - offset;
                    InnerStream.Write(sequence);
                    offset = i + bytesConsumed;
                    count -= bytesConsumed;
                    sequenceFound = true;
                    break;
                }
            }
            if (!sequenceFound) {
                InnerStream.Write(buffer, offset, count);
                count = 0;
            }
        }
    }

    /// <summary>
    /// Tries to decode one or more subsequent Unicode escape sequences into UTF-8 bytes.
    /// </summary>
    /// <param name="buffer">A buffer to decode.</param>
    /// <param name="index">An index to start decoding from.</param>
    /// <param name="result">An array containing UTF-8 representation of the sequence.</param>
    /// <param name="bytesConsumed">The length of the matched escape sequence.</param>
    /// <returns>True if one or more subsequent Unicode escape sequences is found.</returns>
    private static bool DecodeUtf8Sequence(byte[] buffer, int index, out byte[] result, out int bytesConsumed) {
        bytesConsumed = 0;
        result = Array.Empty<byte>();
        List<char> parts = new(2);
        while (DecodeChar(buffer, index, out var part)) {
            parts.Add(part);
            index += 6;
            bytesConsumed += 6;
        }
        if (parts.Count < 1) return false;
        result = Encoding.UTF8.GetBytes(parts.ToArray());
        return true;
    }

    /// <summary>
    /// Tries to decode a single Unicode escape sequence.
    /// </summary>
    /// <remarks>
    /// "\uXXXX" format is assumed for <see cref="Utf8JsonWriter"/> output.
    /// </remarks>
    /// <param name="buffer">A buffer to decode.</param>
    /// <param name="index">An index to start decoding from.</param>
    /// <param name="result">Decoded character.</param>
    /// <returns>True if a single Unicode sequnece is found at specified index.</returns>
    private static bool DecodeChar(byte[] buffer, int index, out char result) {
        result = (char)0;
        if (index + 6 >= buffer.Length || buffer[index] != '\\' || buffer[index + 1] != 'u') return false;
        int charCode = 0;
        for (int i = 0; i < 4; i++)
            if (!DecodeDigit(i, buffer, index + 2, ref charCode)) return false;
        result = (char)charCode;
        return true;
    }

    /// <summary>
    /// Tries to decode a single hexadecimal digit from a buffer.
    /// </summary>
    /// <remarks>
    /// Upper case is assumed for <see cref="Utf8JsonWriter"/> output.
    /// </remarks>
    /// <param name="n">A zero-based digit index.</param>
    /// <param name="buffer">Buffer to decode.</param>
    /// <param name="index">Sequence index.</param>
    /// <param name="charCode">Character code reference.</param>
    /// <returns>True if the buffer contains a hexadecimal digit at <paramref name="index"/> + <paramref name="n"/>.</returns>
    private static bool DecodeDigit(int n, byte[] buffer, int index, ref int charCode) {
        var value = buffer[index + n];
        var shift = 12 - (n << 2);
        if (value is >= 48 and <= 57) charCode += (value - 48) << shift;
        else if (value is >= 65 and <= 70) charCode += (value - 55) << shift;
        else return false;
        return true;
    }

    /// <summary>
    /// Target stream.
    /// </summary>
    private readonly Stream InnerStream;

}