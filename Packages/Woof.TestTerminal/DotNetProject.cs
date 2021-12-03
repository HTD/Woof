using System.Xml;

namespace Woof.Internals;

/// <summary>
/// Metadata for .NET project files.
/// </summary>
public sealed class DotNetProject {

    /// <summary>
    /// Gets the directory where the project file is.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// Gets the project file name without the extension.
    /// </summary>
    public string FileBaseName { get; }

    /// <summary>
    /// Gets the SDK type.
    /// </summary>
    public string Sdk => Project.GetAttribute(nameof(Sdk));

    /// <summary>
    /// Gets the output type.
    /// </summary>
    public string OutputType => TextAt(nameof(OutputType)) ?? "Exe";

    /// <summary>
    /// Gets the TargetFramework string.
    /// </summary>
    public string TargetFramework => TextAt(nameof(TargetFramework)) ?? throw new NullReferenceException();

    /// <summary>
    /// Gets the configured AssemblyName.
    /// </summary>
    public string? AssemblyName => TextAt(nameof(AssemblyName)) is string name && name.Length > 0 ? name : FileBaseName;

    /// <summary>
    /// Gets the base output path.
    /// </summary>
    public string BaseOutputPath => TextAt(nameof(BaseOutputPath)) is string path && path.Length > 0 ? path : "bin";

    /// <summary>
    /// Gets the effective project output directory for the current configuration. Depends on the VS settings.
    /// </summary>
    public string OutputDirectory => Path.Combine(Directory, BaseOutputPath, DotNetSolution.CurrentConfiguration, TargetFramework);

    /// <summary>
    /// Creates the metadata instance for the project file.
    /// </summary>
    /// <param name="path">A path to a project file.</param>
    public DotNetProject(string path) {
        Directory = Path.GetDirectoryName(path) ?? throw new NullReferenceException();
        FileBaseName = Path.GetFileNameWithoutExtension(path);
        Document = new XmlDocument();
        Document.Load(path);
        Project = Document.DocumentElement ?? throw new NullReferenceException();
        if (Sdk != SupportedSdk) throw new NotSupportedException();
    }

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
