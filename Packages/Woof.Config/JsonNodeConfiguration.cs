namespace Woof.Config;

/// <summary>
/// <see cref="IConfiguration"/> directly bound to a JSON object.
/// </summary>
public class JsonNodeConfiguration : JsonNodeSection {

    /// <summary>
    /// Creates the instance from a token.
    /// </summary>
    /// <param name="node">JSON token.</param>
    protected JsonNodeConfiguration(JsonNode? node) : base(node) { }

    /// <summary>
    /// Creates a new <see cref="JsonNodeConfiguration"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    public JsonNodeConfiguration(string json, bool caseSensitive = false) : base(Parse(json, caseSensitive)) { }

    /// <summary>
    /// Creates a new <see cref="JsonNodeConfiguration"/> from a JSON stream.
    /// The stream gets closed.
    /// </summary>
    /// <param name="stream">A stream containing JSON text.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    public JsonNodeConfiguration(Stream stream, bool caseSensitive = false) : base(Parse(stream, caseSensitive)) { }

    /// <summary>
    /// Loads a new <see cref="JsonNodeConfiguration"/> from a JSON file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns><see cref="IConfiguration"/> instance.</returns>
    public static JsonNodeConfiguration Load(string path, bool caseSensitive = false) {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return Load(fileStream, caseSensitive);
    }

    /// <summary>
    /// Loads a new <see cref="JsonNodeConfiguration"/> from a JSON stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns><see cref="IConfiguration"/> instance.</returns>
    public static JsonNodeConfiguration Load(Stream stream, bool caseSensitive = false) => new(Parse(stream, caseSensitive));

    /// <summary>
    /// Loads a new <see cref="JsonNodeConfiguration"/> from a JSON file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <see cref="IConfiguration"/> instance.</returns>
    public static async ValueTask<JsonNodeConfiguration> LoadAsync(string path, bool caseSensitive = false) {
        await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return await LoadAsync(fileStream, caseSensitive);
    }

    /// <summary>
    /// Creates a new <see cref="JsonNodeConfiguration"/> from a stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <see cref="IConfiguration"/> instance.</returns>
    public static async ValueTask<JsonNodeConfiguration> LoadAsync(Stream stream, bool caseSensitive = false) {
        using var document = await JsonDocument.ParseAsync(stream, DefaultDocumentOptions);
        return new(JsonObject.Create(document.RootElement, new JsonNodeOptions { PropertyNameCaseInsensitive = !caseSensitive }));
    }

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    public virtual void Save(string path) {
        if (Node is null) return;
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var writer = new Utf8JsonWriter(fileStream, DefaultWriterOptions);
        Node.WriteTo(writer, DefaultSerializerOptions);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="stream">Writeable stream.</param>
    public virtual void Save(Stream stream) {
        if (Node is null) return;
        using var writer = new Utf8JsonWriter(stream, DefaultWriterOptions);
        Node.WriteTo(writer, DefaultSerializerOptions);
    }

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    public virtual async ValueTask SaveAsync(string path) {
        if (Node is null) return;
        await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var writer = new Utf8JsonWriter(fileStream, DefaultWriterOptions);
        Node.WriteTo(writer, DefaultSerializerOptions);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="stream">Writeable stream.</param>
    public virtual async ValueTask SaveAsync(Stream stream) {
        if (Node is null) return;
        await using var writer = new Utf8JsonWriter(stream, DefaultWriterOptions);
        Node.WriteTo(writer, DefaultSerializerOptions);
    }

    /// <summary>
    /// Parses the JSON text into the root node.
    /// </summary>
    /// <param name="json">JSON text.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>Root node.</returns>
    protected static JsonNode? Parse(string json, bool caseSensitive = false) {
        JsonDocument document = JsonDocument.Parse(json, DefaultDocumentOptions);
        return JsonObject.Create(document.RootElement, new JsonNodeOptions { PropertyNameCaseInsensitive = !caseSensitive });
    }

    /// <summary>
    /// Parses the JSON stream into the root node.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive key matching.</param>
    /// <returns>Root node.</returns>
    protected static JsonNode? Parse(Stream stream, bool caseSensitive = false) {
        JsonDocument document = JsonDocument.Parse(stream, DefaultDocumentOptions);
        return JsonObject.Create(document.RootElement, new JsonNodeOptions { PropertyNameCaseInsensitive = !caseSensitive });
    }

    /// <summary>
    /// Default options for parsing JSON files.
    /// </summary>
    protected static readonly JsonDocumentOptions DefaultDocumentOptions
        = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    /// <summary>
    /// Default options for writing JSON files.
    /// </summary>
    protected static readonly JsonWriterOptions DefaultWriterOptions
        = new() { Indented = true, SkipValidation = true };

    /// <summary>
    /// Default options for the JSON serializer.
    /// </summary>
    protected static readonly JsonSerializerOptions DefaultSerializerOptions
        = new() { WriteIndented = true };

}