namespace Woof.DotNet;

/// <summary>
/// Basic metadata for Visual Studio solution.
/// </summary>
public class DotNetSolution {

    /// <summary>
    /// Gets the current solution file information.
    /// </summary>
    public static FileInfo? CurrentSolutionFile => Executable.Directory.FindBack("*.sln");

    /// <summary>
    /// Gets the solution file information.
    /// </summary>
    public FileInfo File { get; }

    /// <summary>
    /// Gets the all projects that exist in the solution.
    /// </summary>
    public IEnumerable<Project> Projects {
        get {
            if (_Projects is not null) return _Projects;
            _Projects = [];
            if (!File.Exists) throw new FileNotFoundException();
            using var reader = new StreamReader(
                File.FullName, Encoding.UTF8, false,
                new FileStreamOptions {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.ReadWrite
                }
            );
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                if (line?.StartsWith("Global") == true) break;
                if (line is not null && TryParseProjectLine(line, out var project) && project.Path.EndsWith(".csproj")) _Projects.Add(project);
            }
            return _Projects;
        }
    }

    /// <summary>
    /// Creates new basic solution metadata.
    /// </summary>
    /// <param name="file">Solution file.</param>
    public DotNetSolution(FileInfo file) => File = file;

    /// <summary>
    /// Parses solution file line searching for project items.
    /// </summary>
    /// <param name="line">Line to parse.</param>
    /// <param name="project">Project found.</param>
    /// <returns>True if project line found.</returns>
    private bool TryParseProjectLine(string line, out Project project) {
        project = default!;
        if (!line.StartsWith("Project")) return false;
        int index = 8;
        if (!TryMatchString(line, ref index, out var typeId)) return false;
        if (!TryMatchChar(line, ref index, '=')) return false;
        if (!TryMatchString(line, ref index, out var name)) return false;
        if (!TryMatchChar(line, ref index, ',')) return false;
        if (!TryMatchString(line, ref index, out var path)) return false;
        if (!TryMatchChar(line, ref index, ',')) return false;
        if (!TryMatchString(line, ref index, out var guid)) return false;
        project = new Project {
            TypeId = new Guid(typeId),
            Name = name,
            Path = Path.Combine(File.Directory!.FullName, path),
            Guid = new Guid(guid)
        };
        return true;
    }

    /// <summary>
    /// Finds a character from the specified index in the line, advances the index past the character found.
    /// </summary>
    /// <param name="line">Line to search.</param>
    /// <param name="index">Index reference.</param>
    /// <param name="c">Character to find.</param>
    /// <returns>True if the character is found.</returns>
    private static bool TryMatchChar(string line, ref int index, char c) {
        index = line.IndexOf(c, index); if (index < 0) return false; index++;
        return true;
    }

    /// <summary>
    /// Finds a string between two double quote characters. No escaping allowed.
    /// </summary>
    /// <param name="line">Line to search.</param>
    /// <param name="index">Index reference.</param>
    /// <param name="match">Matched string.</param>
    /// <returns>True if the string is found.</returns>
    private static bool TryMatchString(string line, ref int index, out string match) {
        match = default!;
        var start = line.IndexOf('"', index) + 1; if (start < 0) return false; index = start + 1;
        var end = line.IndexOf('"', index); if (end < 0) return false; index = end;
        match = line[start..index]; index = end + 1;
        return true;
    }

#pragma warning disable CS8618

    /// <summary>
    /// Defines a Visual Studio solution project.
    /// </summary>
    public record Project {

        /// <summary>
        /// Gets the project type identifier.
        /// </summary>
        public Guid TypeId { get; init; }

        /// <summary>
        /// Gets the project name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the relative path to the project.
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        public Guid Guid { get; init; }

    }

#pragma warning restore CS8618

    /// <summary>
    /// <see cref="Projects"/> backing field.
    /// </summary>
    private List<Project>? _Projects;

}