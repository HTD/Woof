using System.Text;
using Woof.Internals;

namespace CppMerge;

/// <summary>
/// A list that contains all dependency graph for a C/C++ codebase.
/// </summary>
internal class Codebase : List<CodeFile> {

    /// <summary>
    /// Gets or sets a project root directory.
    /// </summary>
    public string Dir { get; set; } = ".";

    /// <summary>
    /// Gets a root Code file if any.
    /// </summary>
    public CodeFile? Root { get; private set; }

    /// <summary>
    /// Returns a collection of code items in order of dependency, where the root is at the end.
    /// </summary>
    public IEnumerable<CodeFile> DependencyChain {
        get {
            foreach (var item in this.TraversePostOrder(i => i.Vertices).Where(i => i.IsHeader).Distinct()) {
                yield return item;
                if (item.Implementation is not null) yield return item.Implementation;
            }
        }
    }

    /// <summary>
    /// Loads a code base from files.
    /// </summary>
    /// <param name="directory">Target directory.</param>
    /// <param name="file">Root file name or relative path.</param>
    /// <exception cref="ArgumentException">Directory doesn't exist or the file is not found.</exception>
    public void Load(string directory, string? file = null) {
        if (!Directory.Exists(directory)) throw new ArgumentException("Directory doesn't exist");
        Dir = Path.GetFullPath(directory);
        if (file is not null) {
            var rootPathRelative = FindFile(file);
            if (rootPathRelative is not null) {
                var rootPathAbsolute = Path.Combine(Dir, rootPathRelative);
                if (File.Exists(rootPathAbsolute)) {
                    Root = new CodeFile(this, rootPathRelative);
                    Add(Root);
                    Root.Parse();
                    return;
                }
            }
            throw new ArgumentException("File doesn't exist");
        }
        foreach (var relativePath in GetPaths()) Add(new(this, relativePath));
        while (this.FirstOrDefault(i => !i.IsParsed) is CodeFile unparsed) unparsed.Parse();
    }

    /// <summary>
    /// Fills a clipboard with all the relevant code wrapped in markdown.
    /// </summary>
    public void BuildClipboardContent() {
        StringBuilder builder = new();
        builder.AppendLine("Codebase:");
        foreach (var item in DependencyChain) {
            builder.AppendLine();
            builder.AppendLine($"{item.RelativePath}:");
            builder.AppendLine($"```{item.MarkdownType}");
            builder.AppendLine(item.Content);
            builder.AppendLine("```");
        }
        Clipboard.SetText(builder.ToString());
    }

    /// <summary>
    /// Gets the collection of all relative paths in the project directory that match the C/C++ file search pattern.
    /// </summary>
    /// <returns>A collection of relative paths.</returns>
    private IEnumerable<string> GetPaths() => (
                    from pattern in (string[])["*.c", "*.cpp", "*.h", "*.hpp"]
                    from file in Directory.EnumerateFiles(Dir, pattern, SearchOption.AllDirectories)
                    select Path.GetRelativePath(Dir, file)
                ).Distinct();

    internal static string NormalizePath(string path) =>
        Path.DirectorySeparatorChar != '/' ? path.Replace('/', Path.DirectorySeparatorChar) : path;

    /// <summary>
    /// Finds a file that lives somewhere below the `Dir`.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns>Relative path to the file if found, null otherwise.</returns>
    internal string? FindFile(string fileName) {
        var normalized = NormalizePath(fileName);
        var absolute = Path.Combine(Dir, normalized);
        if (File.Exists(absolute)) return normalized;
        return
            Directory.EnumerateFiles(Dir, Path.GetFileName(fileName), SearchOption.AllDirectories).Select(f => Path.GetRelativePath(Dir, f)).FirstOrDefault();
    }

    internal string? FindImplementation(string fileName) {
        var target = NormalizePath(fileName);
        if (Path.GetExtension(target) == ".h") target = Path.ChangeExtension(target, ".c");
        if (Path.GetExtension(target) == ".hpp") target = Path.ChangeExtension(target, ".cpp");
        return FindFile(target);
    }

    /// <summary>
    /// Gets the file from this collection matched by the file name alone. It assumes all subdirectories are in include path.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    internal CodeFile? GetByFileName(string fileName) => this.FirstOrDefault(f => Path.GetFileName(f.RelativePath) == Path.GetFileName(NormalizePath(fileName)));

    /// <summary>
    /// Gets the file from this collection matched by relative path.
    /// </summary>
    /// <param name="relativePath">Relative path to match.</param>
    /// <returns>File instance or null if not found.</returns>
    internal CodeFile? GetByRelativePath(string relativePath) => this.FirstOrDefault(f => f.RelativePath == NormalizePath(relativePath));
        
}
