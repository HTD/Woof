using System;
using System.IO;

namespace Test.Api;

public class Helpers {

    /// <summary>
    /// Gets a pseudo-random test data stream.
    /// </summary>
    /// <param name="length">Target length.</param>
    /// <param name="seed">PRNG seed.</param>
    /// <returns>A memory stream containing pseudo random sequence of bytes.</returns>
    public static MemoryStream GetTestDataStream(int length, int seed) {
        var buffer = new byte[length];
        new Random(seed).NextBytes(buffer);
        return new MemoryStream(buffer);
    }

    /// <summary>
    /// Corrupts a binary key in Base64 form by changing a random byte of it.
    /// </summary>
    /// <param name="key">A binary key in Base64 form.</param>
    /// <returns>Corrupted key.</returns>
    public static string CorruptBase64Key(string key) {
        var keyData = Convert.FromBase64String(key);
        var prng = new Random();
        var offset = prng.Next(keyData.Length);
        var value = keyData[offset];
        while (keyData[offset] == value) prng.NextBytes(keyData.AsSpan(offset, 1));
        return Convert.ToBase64String(keyData);
    }

}
