namespace Woof;

/// <summary>
/// Universal file system helpers for both Linux and Windows.
/// </summary>
public static class FileSystem {

    /// <summary>
    /// Copies all directory content to a new location.
    /// </summary>
    /// <param name="sourceDirectory">Source directory.</param>
    /// <param name="targetDirectory">Target directory.</param>
    /// <param name="pattern">Search pattern.</param>
    /// <param name="overwrite">True to overwrite existing files.</param>
    /// <returns>True if all entries are copied properly.</returns>
    public static bool CopyDirectoryContent(string sourceDirectory, string targetDirectory, string pattern = "*", bool overwrite = true) {
        var ok = true;
        var sourceDirectoryInfo = new DirectoryInfo(sourceDirectory);
        var targetDirectoryInfo = Directory.CreateDirectory(targetDirectory);
        sourceDirectory = sourceDirectoryInfo.FullName;
        targetDirectory = targetDirectoryInfo.FullName;
        var entries = sourceDirectoryInfo.EnumerateFileSystemInfos(pattern, SearchOption.AllDirectories);
        var sourceRelativePathOffset = sourceDirectory.Length;
        foreach (var entry in entries) {
            try {
                var targetRelative = entry.FullName[sourceRelativePathOffset..].TrimStart(Path.DirectorySeparatorChar);
                var targetAbsolute = Path.Combine(targetDirectory, targetRelative);
                if (entry.Attributes.HasFlag(FileAttributes.Directory)) Directory.CreateDirectory(targetAbsolute);
                else File.Copy(entry.FullName, targetAbsolute, overwrite);
            }
            catch {
                ok = false;
            }
        }
        return ok;
    }

}