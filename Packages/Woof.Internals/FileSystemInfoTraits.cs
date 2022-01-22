namespace Woof.Internals;

/// <summary>
/// Tools for searching related files.
/// </summary>
public static class FileSystemInfoTraits {

    /// <summary>
    /// Finds a file searching back from the source file.
    /// </summary>
    /// <param name="source">Source file information.</param>
    /// <param name="searchPattern">A search pattern for the file to find.</param>
    /// <returns>Matched file information or null if not found.</returns>
    public static FileInfo? FindBack(this FileInfo source, string searchPattern)
        => source.Directory?.FindBack(searchPattern);

    /// <summary>
    /// Finds a file matching the pattern searching back form this directory to the root.
    /// </summary>
    /// <param name="directory">A directory to start the search in.</param>
    /// <param name="searchPattern">A search pattern for the file to find.</param>
    /// <returns>A matched file information or null if not found.</returns>
    public static FileInfo? FindBack(this DirectoryInfo directory, string searchPattern) {
        FileInfo? match = null;
        while (match is null && directory is not null) {
            match = directory?.GetFiles(searchPattern).FirstOrDefault();
            if (match is not null) return match;
            if (directory is not null) directory = directory.Parent!;
        }
        return null;
    }

}