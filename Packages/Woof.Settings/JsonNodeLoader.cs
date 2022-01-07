namespace Woof.Settings;

/// <summary>
/// Implements the default loader for the <see cref="JsonNode"/> type.
/// </summary>
public class JsonNodeLoader : ILoader<JsonNode> {

    /// <summary>
    /// Gets the default singleton instance of the default loader.
    /// </summary>
    public static JsonNodeLoader Default => _Default ??= new JsonNodeLoader();

    /// <summary>
    /// Gets the default options for parsing JSON files.
    /// </summary>
    public JsonDocumentOptions DocumentOptions { get; } = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    /// <summary>
    /// Gets the default options for writing JSON files.
    /// </summary>
    public JsonWriterOptions WriterOptions { get; } = new() { Indented = true, SkipValidation = true };

    /// <summary>
    /// Gets the default options for the JSON serializer.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; } = new() { WriteIndented = true };

    /// <summary>
    /// Parses the JSON text into the root node.
    /// </summary>
    /// <param name="json">JSON text.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>Root node.</returns>
    public JsonNode Parse(string json, bool caseSensitive = false)
        => JsonObject.Create(JsonDocument.Parse(json, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive)!;

    /// <summary>
    /// Loads the root node from file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    public virtual JsonNode Load(string path, bool caseSensitive = false) {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return Load(stream, caseSensitive);
    }

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    public virtual JsonNode Load(Stream stream, bool caseSensitive = false)
        => JsonObject.Create(JsonDocument.Parse(stream, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive)!;

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    public virtual async ValueTask<JsonNode> LoadAsync(string path, bool caseSensitive = false) {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return JsonObject.Create((await JsonDocument.ParseAsync(stream, DocumentOptions)).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive)!;
    }

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    public virtual async ValueTask<JsonNode> LoadAsync(Stream stream, bool caseSensitive = false)
        => JsonObject.Create((await JsonDocument.ParseAsync(stream, DocumentOptions)).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive)!;

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public virtual void Save(JsonNode node, string path) {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        Save(node, stream);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public virtual void Save(JsonNode node, Stream stream) {
        if (node is null) return;
        using var writer = new Utf8JsonWriter(stream, WriterOptions);
        node.WriteTo(writer, SerializerOptions);
    }

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public virtual async ValueTask SaveAsync(JsonNode node, string path) {
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await SaveAsync(node, stream);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public virtual async ValueTask SaveAsync(JsonNode node, Stream stream) {
        if (node is null) return;
        await using var writer = new Utf8JsonWriter(stream, WriterOptions);
        node.WriteTo(writer, SerializerOptions);
    }

    /// <summary>
    /// <see cref="JsonNodeOptions"/> for case sensitive property names.
    /// </summary>
    protected readonly JsonNodeOptions CaseSensitive = new() { PropertyNameCaseInsensitive = false };

    /// <summary>
    /// <see cref="JsonNodeOptions"/> for case insensitive property names.
    /// </summary>
    protected readonly JsonNodeOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// The <see cref="Default"/> property backing field.
    /// </summary>
    private static JsonNodeLoader? _Default;

}