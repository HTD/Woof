namespace Woof.Internals;

/// <summary>
/// Metadata for .NET project files.
/// </summary>
public sealed class DotNetProject {

    /// <summary>
    /// Gets the project directory information.
    /// </summary>
    public DirectoryInfo Directory { get; }

    /// <summary>
    /// Gets the project file information.
    /// </summary>
    public FileInfo FileInfo { get; }

    /// <summary>
    /// Gets the project file name without the extension.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the SDK type.
    /// </summary>
    public string Sdk { get; }

    /// <summary>
    /// Gets the output type.
    /// </summary>
    public string OutputType { get; }

    /// <summary>
    /// Gets the TargetFramework string.
    /// </summary>
    public string TargetFramework { get; }

    /// <summary>
    /// Gets the configured AssemblyName.
    /// </summary>
    public string? AssemblyName { get; }

    /// <summary>
    /// Gets the base output path.
    /// </summary>
    public string BaseOutputPath { get; }

    /// <summary>
    /// Gets the effective project output directory for the current configuration. Depends on the VS settings.
    /// </summary>
    public DirectoryInfo OutputDirectory { get; }

    /// <summary>
    /// Creates the metadata instance for the project file.
    /// </summary>
    /// <param name="path">A path to a project file.</param>
    public DotNetProject(string path) {
        Directory = Path.GetDirectoryName(path) is string directory ? new DirectoryInfo(directory)
            : throw new InvalidOperationException("Can't get the base directory for the project");
        FileInfo = new(path);
        FileName = Path.GetFileNameWithoutExtension(path);
        Document = new XmlDocument();
        Document.Load(path);
        Project = Document.DocumentElement
            ?? throw new InvalidOperationException("Can't get the root element for the project");
        Sdk = Project.GetAttribute(nameof(Sdk));
        if (Sdk != SupportedSdk) throw new NotSupportedException("The project SDK is not supported");

        OutputType = TextAt(nameof(OutputType)) ?? "Exe";
        TargetFramework = TextAt(nameof(TargetFramework))
            ?? throw new InvalidOperationException("Can't get the target framework for the project");
        AssemblyName = TextAt(nameof(AssemblyName)) is string name && name.Length > 0 ? name : FileName;
        BaseOutputPath = TextAt(nameof(BaseOutputPath)) is string baseOutputPath && baseOutputPath.Length > 0 ? baseOutputPath : "bin";
        OutputDirectory =
            new DirectoryInfo(Path.Combine(Directory.FullName, BaseOutputPath, Executable.CurrentBuildConfiguration, TargetFramework));
    }

    /// <summary>
    /// Gets the project metadata from a path to any file that belongs to the project.
    /// </summary>
    /// <param name="sourceFileName">A source file name.</param>
    /// <returns>Project metadata or null if not found.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DotNetProject? GetFromSource(string? sourceFileName)
        => sourceFileName is not null && new FileInfo(sourceFileName).FindBack("*.csproj") is FileInfo projectFile
            ? new DotNetProject(projectFile.FullName)
            : null;

    /// <summary>
    /// Gets the text content of the first matched tag in the Project XML element.
    /// </summary>
    /// <param name="tagName">Name of the tag.</param>
    /// <returns>Tag text content if exists, null otherwise.</returns>
    private string? TextAt(string tagName) => Project.GetElementsByTagName(tagName).OfType<XmlElement>().FirstOrDefault()?.InnerText;

    /// <summary>
    /// Current XML document.
    /// </summary>
    private readonly XmlDocument Document;

    /// <summary>
    /// Project XML element.
    /// </summary>
    private readonly XmlElement Project;

    /// <summary>
    /// Supported SDK type.
    /// </summary>
    private const string SupportedSdk = "Microsoft.NET.Sdk";

}
