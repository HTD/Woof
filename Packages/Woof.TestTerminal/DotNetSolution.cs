using System.Reflection;

namespace Woof.Internals;

/// <summary>
/// Visual Studio simple solution helpers.
/// </summary>
public class DotNetSolution {

    /// <summary>
    /// Gets the solution directory by searching directory tree upwards starting from the calling assembly location.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when the solution can't be found starting from the calling location.</exception>
    public static string CurrentSolutionDirectory {
        get {
            var currentPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any()) directory = directory.Parent;
            return directory switch {
                null => throw new NullReferenceException("Couldn't find the solution"),
                _ => directory.ToString()
            };
        }
    }

    /// <summary>
    /// Gets the calling assembly configuration like "Release" or "Debug".
    /// </summary>
    public static string CurrentConfiguration => Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;

}
