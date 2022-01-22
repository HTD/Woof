namespace Woof.Internals;

/// <summary>
/// Visual Studio simple solution helpers.
/// </summary>
public class DotNetSolution {

    /// <summary>
    /// Gets the solution directory by searching directory tree upwards starting from the main executable location.
    /// </summary>
    public static DirectoryInfo? CurrentSolutionDirectory {
        get {
            if (_CurrentSolutionDirectory is not null) return _CurrentSolutionDirectory;
            var directory = Executable.Directory;
            while (directory is not null && !directory.GetFiles("*.sln").Any()) directory = directory.Parent;
            return _CurrentSolutionDirectory = directory;
        }
    }

    /// <summary>
    /// Current solution directory backing field.
    /// </summary>
    private static DirectoryInfo? _CurrentSolutionDirectory;

}