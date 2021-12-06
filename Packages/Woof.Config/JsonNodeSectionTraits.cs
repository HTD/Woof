namespace Woof.Config;

/// <summary>
/// Provides extension methods for saving the <see cref="JsonNodeSection"/> type from the <see cref="JsonNodeLoader"/>.
/// </summary>
public static class JsonNodeSectionTraits {

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public static void Save(this JsonNodeSection node, string path) => node.Loader.Save(node, path);

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public static void Save(this JsonNodeSection node, Stream stream) => node.Loader.Save(node, stream);

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public static ValueTask SaveAsync(this JsonNodeSection node, string path) => node.Loader.SaveAsync(node, path);

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public static ValueTask SaveAsync(this JsonNodeSection node, Stream stream) => node.Loader.SaveAsync(node, stream);

}