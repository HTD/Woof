namespace Woof.Config;

/// <summary>
/// Defines methods for parsing, loading and saving <see cref="JsonNodeSection"/> type.
/// </summary>
public interface IJsonNodeLoader {

    /// <summary>
    /// Gets the options for parsing JSON files.
    /// </summary>
    JsonDocumentOptions DocumentOptions { get; }

    /// <summary>
    /// Gets the options for writing JSON files.
    /// </summary>
    JsonWriterOptions WriterOptions { get; }

    /// <summary>
    /// Gets the options for the JSON serializer.
    /// </summary>
    JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// Parses the JSON text into the root node.
    /// </summary>
    /// <param name="json">JSON text.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>Root node.</returns>
    JsonNodeSection Parse(string json, bool caseSensitive = false);

    /// <summary>
    /// Loads a new <see cref="JsonNodeSection"/> from a JSON file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns><see cref="JsonNodeSection"/> instance.</returns>
    JsonNodeSection Load(string path, bool caseSensitive = false);

    /// <summary>
    /// Loads a new <see cref="JsonNodeSection"/> from a JSON stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns><see cref="JsonNodeSection"/> instance.</returns>
    JsonNodeSection Load(Stream stream, bool caseSensitive = false);

    /// <summary>
    /// Loads a new <see cref="JsonNodeSection"/> from a JSON file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <see cref="JsonNodeSection"/> instance.</returns>
    ValueTask<JsonNodeSection> LoadAsync(string path, bool caseSensitive = false);

    /// <summary>
    /// Creates a new <see cref="JsonNodeSection"/> from a stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <see cref="JsonNodeSection"/> instance.</returns>
    ValueTask<JsonNodeSection> LoadAsync(Stream stream, bool caseSensitive = false);

    /// <summary>
    /// Loads the root node from file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    JsonNode? LoadRoot(string path, bool caseSensitive = false);

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    JsonNode? LoadRoot(Stream stream, bool caseSensitive = false);

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    ValueTask<JsonNode?> LoadRootAsync(string path, bool caseSensitive = false);

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    ValueTask<JsonNode?> LoadRootAsync(Stream stream, bool caseSensitive = false);

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    void Save(JsonNodeSection node, string path);

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    void Save(JsonNodeSection node, Stream stream);

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    ValueTask SaveAsync(JsonNodeSection node, string path);

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    ValueTask SaveAsync(JsonNodeSection node, Stream stream);

}
