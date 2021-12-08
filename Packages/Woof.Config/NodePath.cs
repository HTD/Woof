namespace Woof.Config;

/// <summary>
/// JSON path / <see cref="IConfiguration"/> path tools.
/// </summary>
public class NodePath {

    /// <summary>
    /// Gets the parts as (Key, Path) tuples.
    /// </summary>
    public IEnumerable<(string Key, string Path)> Parts {
        get {
            for (int i = 0, n = Keys.Length; i < n; i++) {
                yield return i == 0
                    ? (Keys[0], Keys[0])
                    : (Keys[i], string.Join(':', Keys[0..(i+1)]));
            }
        }
    }

    /// <summary>
    /// Gets the parent path as (Key, Path) tuple.
    /// </summary>
    public (string Key, string Path) Parent =>
        Keys.Length < 2 ? (string.Empty, string.Empty) :
        Keys.Length < 3 ? (Keys[0], Keys[0]) :
        (Keys[^2], string.Join(':', Keys[0..(Keys.Length - 1)]));

    /// <summary>
    /// Gets the key of the path (last part).
    /// </summary>
    public string Key => Keys[^1];

    /// <summary>
    /// Gets the full section path.
    /// </summary>
    public string Path => string.Join(':', Keys);

    /// <summary>
    /// Creates a path object that can be enumerated as (Key, Path) tuplets.
    /// </summary>
    /// <param name="path">Node path in either JSON or <see cref="IConfiguration"/> form.</param>
    public NodePath(string path) => Keys = Split(path).ToArray();

    /// <summary>
    /// Splits the node path into parts.
    /// </summary>
    /// <param name="path">Node path in either JSON or <see cref="IConfiguration"/> form.</param>
    /// <returns>Parts.</returns>
    public static IEnumerable<string> Split(string path) {
        int n = path.Length;
        if (n < 1) yield break;
        int s = path[0] == '$' ? 2 : 0, p = s;
        if (p > 0 && n < 2) yield break;
        for (int i = s; i < n; i++) {
            if (path[i] is '.' or ':' or '[') {
                if (p > s) p++;
                yield return path[p..i];
                p = i++;
            }
        }
        if (p > s) p++;
        yield return path[n - 1] != ']' ? path[p..] : path[p..^1];
    }

    /// <summary>
    /// Converts a JSON node path to a <see cref="IConfiguration"/> node path.
    /// </summary>
    /// <param name="path">JSON node path.</param>
    /// <returns>Section path.</returns>
    public static string GetSectionPath(string path) => string.Join(':', Split(path));

    /// <summary>
    /// Keys array for the instance.
    /// </summary>
    private readonly string[] Keys;

}