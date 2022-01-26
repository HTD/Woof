namespace Woof.Internals;

/// <summary>
/// Gets the information about the current application main executable file and its directory.
/// </summary>
/// <remarks>
/// For special execution context, like WPF designer context,
/// call <see cref="ResetAssembly{T}(string?)"/> to locate the main executable file from the debug symbols.
/// </remarks>
public static class Executable {

    /// <summary>
    /// Gets or sets the main application assembly.
    /// </summary>
    /// <remarks>
    /// Normally it's the entry assembly, but in some scenarios like tests or designer runs it might need to be reset in code.<br/>
    /// Call <see cref="ResetAssembly{T}"/> in that case.
    /// </remarks>
    public static Assembly Assembly { get; private set; }

    /// <summary>
    /// Gets the build configuration for the current assembly.
    /// </summary>
    public static string CurrentBuildConfiguration { get; private set; }

    /// <summary>
    /// Gets the application base directory.
    /// </summary>
    public static DirectoryInfo Directory { get; private set; }

    /// <summary>
    /// Gets the application executable file information.
    /// </summary>
    public static FileInfo FileInfo { get; private set; }

    /// <summary>
    /// Gets the application executable file path.
    /// </summary>
    public static string FilePath { get; private set; }

    /// <summary>
    /// Gets the application executable file name without the extension.
    /// </summary>
    public static string FileName { get; private set; }

    /// <summary>
    /// Initializes the default values for the application.
    /// </summary>
    static Executable() {
        Assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        CurrentBuildConfiguration = Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        Directory = new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!.TrimEnd(Path.DirectorySeparatorChar));
        FilePath = Process.GetCurrentProcess()?.MainModule?.FileName!;
        FileInfo = new(FilePath);
        FileName = Path.GetFileNameWithoutExtension(FileInfo.Name);
    }

    /// <summary>
    /// Resets the application main assembly to the one that contains type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// If the assembly is already set to the correct value, this call is ignored.<br/>
    /// </remarks>
    /// <typeparam name="T">A type contain in the main application assembly.</typeparam>
    /// <param name="sourceFilePath">A path to the caller file provided automatically by the compiler if debug symbols are available. Do not set.</param>
    /// <exception cref="InvalidOperationException">Anything goes wrong.</exception>
    public static void ResetAssembly<T>([CallerFilePath] string? sourceFilePath = default) {
        var target = Assembly.GetAssembly(typeof(T)) ?? throw new InvalidOperationException($"Can't get assembly for type {typeof(T).Name}");
        if (target.FullName == Assembly.FullName) return;
        Assembly = target;
        CurrentBuildConfiguration = Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        var currentProject = DotNetProject.GetFromSource(sourceFilePath);
        Directory = currentProject is not null
            ? currentProject.OutputDirectory
            : new DirectoryInfo(Path.GetDirectoryName(Assembly.Location)!.TrimEnd(Path.DirectorySeparatorChar));
        FilePath = Path.Combine(Directory.FullName, Path.GetFileName(Assembly.Location));
        FileInfo = new(FilePath);
        FileName = Path.GetFileNameWithoutExtension(FileInfo.Name);
        System.IO.Directory.SetCurrentDirectory(Directory.FullName);
    }

}