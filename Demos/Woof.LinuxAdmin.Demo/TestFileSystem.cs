using System.Text;
using System.Text.RegularExpressions;

namespace Woof.LinuxAdmin.Demo;

public static partial class TestFileSystem {

    /// <summary>
    /// Creates test file structure.
    /// </summary>
    /// <param name="path">Target path.</param>
    /// <param name="files">Number of files in each subdirectory.</param>
    /// <param name="directories">Number of directories in each subdirectory.</param>
    /// <param name="depth">Subdirectory structure depth.</param>
    public static void CreateTestStructure(string path, int files = 3, int directories = 3, int depth = 3) {
        Directory.CreateDirectory(path);
        foreach (var entry in TestStructure(files, directories, depth)) {
            var target = Path.Combine(path, entry.Path);
            if (entry.IsDirectory) Directory.CreateDirectory(target);
            else File.WriteAllText(target, target);
        }
    }

    /// <summary>
    /// Creates file entries tree in BFS order.
    /// </summary>
    /// <param name="files">Number of files in each subdirectory.</param>
    /// <param name="directories">Number of directories in each subdirectory.</param>
    /// <param name="depth">Subdirectory structure depth.</param>
    /// <returns>BFS enumeration of test structure.</returns>
    private static IEnumerable<(string Path, bool IsDirectory)> TestStructure(int files = 3, int directories = 3, int depth = 3) {
        Queue<(string path, bool isDirectory, int level)> q = new();
        for (int i = 0; i < directories; i++) q.Enqueue(GetItem(null, true, i + 1, 0));
        for (int i = 0; i < files; i++) q.Enqueue(GetItem(null, false, i + 1, 0));
        while (q.TryDequeue(out var item)) {
            yield return (item.path, item.isDirectory);
            if (item.isDirectory && item.level < depth) {
                for (int i = 0; i < directories; i++) q.Enqueue(GetItem(item.path, true, i + 1, item.level + 1));
                for (int i = 0; i < files; i++) q.Enqueue(GetItem(item.path, false, i + 1, item.level + 1));
            }
        }
    }

    /// <summary>
    /// Create test file system entry tuple.
    /// </summary>
    /// <param name="parent">Parent directory.</param>
    /// <param name="isDirectory">Is current entry a directory.</param>
    /// <param name="index">Entry index.</param>
    /// <param name="level">Entry level.</param>
    /// <returns>Test file system entry tuple.</returns>
    private static (string Path, bool IsDirectory, int Level) GetItem(string? parent, bool isDirectory, int index, int level) {
        StringBuilder builder = new();
        string itemType = isDirectory ? "dir" : "file";
        if (parent is null) {
            builder.Append(itemType);
            builder.Append(index);
        }
        else {
            builder.Append(parent);
            builder.Append(Path.DirectorySeparatorChar);
            builder.Append(itemType);
            builder.Append(GetNumber(parent));
            builder.Append(index);
        }
        return (builder.ToString(), isDirectory, level);
    }

    /// <summary>
    /// Gets number from the end of the string.
    /// </summary>
    /// <param name="path">Path ending with a number.</param>
    /// <returns>Number at the end.</returns>
    private static int GetNumber(string path) {
        return int.Parse(RxNumber.Match(path).Value);
    }

    /// <summary>
    /// A regular expression matching numbers at the end of the string.
    /// </summary>
    private static readonly Regex RxNumber = RxNumberCT();

    [GeneratedRegex("\\d+$", RegexOptions.Compiled)]
    private static partial Regex RxNumberCT();

}