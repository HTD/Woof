namespace Woof.Config;

/// <summary>
/// JSON path / <see cref="IConfiguration"/> path tools.
/// </summary>
public static class NodePath {

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

}
