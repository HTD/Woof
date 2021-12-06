namespace Woof.Config;

/// <summary>
/// Implements the default loader for the <see cref="JsonNodeSection"/> type.
/// </summary>
public class JsonNodeLoader : IJsonNodeLoader {

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
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>Root node.</returns>
    public JsonNodeSection Parse(string json, bool caseSensitive = false) {
        var root = JsonObject.Create(JsonDocument.Parse(json, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive);
        return root is null ? JsonNodeSection.Empty : new(root);
    }

    /// <summary>
    /// Loads a new <see cref="JsonNodeSection"/> from a JSON file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns><see cref="JsonNodeSection"/> instance.</returns>
    public JsonNodeSection Load(string path, bool caseSensitive = false)
        => LoadRoot(path, caseSensitive) is JsonNode root ? new(root) : JsonNodeSection.Empty;

    /// <summary>
    /// Loads a new <see cref="JsonNodeSection"/> from a JSON stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns><see cref="JsonNodeSection"/> instance.</returns>
    public JsonNodeSection Load(Stream stream, bool caseSensitive = false)
        => LoadRoot(stream, caseSensitive) is JsonNode root ? new(root) : JsonNodeSection.Empty;

    /// <summary>
    /// Loads a new <see cref="JsonNodeSection"/> from a JSON file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <see cref="JsonNodeSection"/> instance.</returns>
    public async ValueTask<JsonNodeSection> LoadAsync(string path, bool caseSensitive = false)
        => (await LoadRootAsync(path, caseSensitive)) is JsonNode root ? new(root) : JsonNodeSection.Empty;

    /// <summary>
    /// Creates a new <see cref="JsonNodeSection"/> from a stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <see cref="JsonNodeSection"/> instance.</returns>
    public async ValueTask<JsonNodeSection> LoadAsync(Stream stream, bool caseSensitive = false)
        => (await LoadRootAsync(stream, caseSensitive)) is JsonNode root ? new(root) : JsonNodeSection.Empty;

    /// <summary>
    /// Loads the root node from file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    public JsonNode? LoadRoot(string path, bool caseSensitive = false) {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return LoadRoot(stream, caseSensitive);
    }

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    public virtual JsonNode? LoadRoot(Stream stream, bool caseSensitive = false)
        => JsonObject.Create(JsonDocument.Parse(stream, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive);

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    public async ValueTask<JsonNode?> LoadRootAsync(string path, bool caseSensitive = false) {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return JsonObject.Create((await JsonDocument.ParseAsync(stream, DocumentOptions)).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive);
    }

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    public virtual async ValueTask<JsonNode?> LoadRootAsync(Stream stream, bool caseSensitive = false)
        => JsonObject.Create((await JsonDocument.ParseAsync(stream, DocumentOptions)).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive);

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public void Save(JsonNodeSection node, string path) {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        Save(node, stream);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public virtual void Save(JsonNodeSection node, Stream stream) {
        if (node.IsNullOrEmpty) return;
        using var writer = new Utf8JsonWriter(stream, WriterOptions);
        node.Node?.WriteTo(writer, SerializerOptions);
    }

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public async ValueTask SaveAsync(JsonNodeSection node, string path) {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        await SaveAsync(node, stream);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNodeSection"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public virtual async ValueTask SaveAsync(JsonNodeSection node, Stream stream) {
        if (node.IsNullOrEmpty) return;
        await using var writer = new Utf8JsonWriter(stream, WriterOptions);
        node.Node?.WriteTo(writer, SerializerOptions);
    }

    /// <summary>
    /// <see cref="JsonNodeOptions"/> for case sensitive property names.
    /// </summary>
    protected readonly JsonNodeOptions CaseSensitive = new() { PropertyNameCaseInsensitive = false };

    /// <summary>
    /// <see cref="JsonNodeOptions"/> for case insensitive property names.
    /// </summary>
    protected readonly JsonNodeOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };

}