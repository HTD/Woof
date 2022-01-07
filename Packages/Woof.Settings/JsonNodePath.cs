namespace Woof.Settings;

/// <summary>
/// JSON path as an array of keys providing methods for accessing its parts.
/// </summary>
public class JsonNodePath {

    /// <summary>
    /// Gets an empty path - a path containing no keys.
    /// </summary>
    public static readonly JsonNodePath Empty = new(Array.Empty<string>());

    /// <summary>
    /// Gets the key as an index if it is an index. Otherwise gets -1.
    /// </summary>
    public int Index => IsIndex && int.TryParse(Key, out var index) ? index : -1;

    /// <summary>
    /// Gets a value indicating the path is empty.
    /// </summary>
    public bool IsEmpty => Keys.Length == 0;

    /// <summary>
    /// Gets a value indicating that the key of the path is an array index.
    /// </summary>
    public bool IsIndex => Key.Length > 0 && Key[0] is >='0' and <= '9';

    /// <summary>
    /// Gets the key of the path (last part).
    /// </summary>
    public string Key => Keys[^1];

    /// <summary>
    /// Gets the keys array for the instance.
    /// </summary>
    public string[] Keys { get; }

    /// <summary>
    /// Gets the number of keys in the path.
    /// </summary>
    public int Length => Keys.Length;

    /// <summary>
    /// Gets the parent path.
    /// </summary>
    public JsonNodePath? Parent => Keys.Length < 2 ? null : new JsonNodePath(Keys[0..(Keys.Length - 1)]);

    /// <summary>
    /// Gets the parts of the path.
    /// </summary>
    public IEnumerable<JsonNodePath> Parts => Enumerable.Range(0, Keys.Length).Select<int, JsonNodePath>(i => new(Keys[0..(i + 1)]));

    /// <summary>
    /// Creates the path from a string.
    /// </summary>
    /// <param name="path">Node path.</param>
    public JsonNodePath(string? path) => Keys = string.IsNullOrEmpty(path) ? Array.Empty<string>() : Split(path).ToArray();

    /// <summary>
    /// Creates the path from a string path and an additional key.
    /// </summary>
    /// <param name="path">Node parent path.</param>
    /// <param name="key">Node key (a property name or an index).</param>
    public JsonNodePath(string? path, string key)
        => Keys = string.IsNullOrEmpty(path) ? new string[] { key } : Split(path).Append(key).ToArray();

    /// <summary>
    /// Creates the path from the path object and an additional key.
    /// </summary>
    /// <param name="path">Node parent path.</param>
    /// <param name="key">Node key (a property name or an index).</param>
    public JsonNodePath(JsonNodePath path, string key)
        => Keys = path.Length < 1 ? new string[] { key } : path.Keys.Append(key).ToArray();

    /// <summary>
    /// Creates the path from keys.
    /// </summary>
    /// <param name="keys">Separate path keys.</param>
    public JsonNodePath(string[] keys) => Keys = keys;

    /// <summary>
    /// Gets the path as string.
    /// </summary>
    /// <returns>Node path.</returns>
    public override string ToString() => Join(Keys);

    /// <summary>
    /// Splits the node path into parts.
    /// </summary>
    /// <param name="path">Node path.</param>
    /// <returns>The separate keys.</returns>
    public static IEnumerable<string> Split(string path) {
        int n = path.Length;
        if (n < 1) yield break;
        int s = 0, p = s;
        if (p > 0 && n < 2) yield break;
        for (int i = s; i < n; i++) {
            if (path[i] is '.' or '[') {
                if (p > s) p++;
                yield return path[p..i];
                p = i++;
            }
        }
        if (p > s) p++;
        yield return path[n - 1] != ']' ? path[p..] : path[p..^1];
    }

    /// <summary>
    /// Joins the node path from the parts.
    /// </summary>
    /// <param name="parts">Keys of the path.</param>
    /// <returns>The joined path.</returns>
    public static string Join(IEnumerable<string> parts) {
        var builder = new StringBuilder();
        bool isIndex;
        bool isNext = false;
        foreach (var part in parts) {
            isIndex = part.Length > 0 && part[0] is >= '0' and <= '9';
            if (isNext) builder.Append(isIndex ? '[' : '.');
            builder.Append(part);
            if (isIndex) builder.Append(']');
            isNext = true;
        }
        return builder.ToString();
    }

    /// <summary>
    /// Implicit conversion from string.
    /// </summary>
    /// <param name="path">String path.</param>
    public static implicit operator JsonNodePath(string path) => new(path);

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    /// <param name="path"></param>
    public static implicit operator string(JsonNodePath path) => path.ToString();

    /// <summary>
    /// Calculates the hash code for the path.
    /// </summary>
    /// <returns>Calculated hash code for all keys.</returns>
    public override int GetHashCode() => Keys.Aggregate(0, (k, h) => HashCode.Combine(k, h));

    /// <summary>
    /// Tests if the other path is equal to this path.
    /// </summary>
    /// <param name="obj">Other path.</param>
    /// <returns>True if equal.</returns>
    public override bool Equals(object? obj)
        => obj is JsonNodePath path && path.Keys.Length == Keys.Length && path.Keys.SequenceEqual(Keys, StringComparer.OrdinalIgnoreCase);

}