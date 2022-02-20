namespace Woof.Settings;

/// <summary>
/// Implements the default loader for the <see cref="JsonNode"/> type.
/// </summary>
public class JsonNodeProtectedLoader : JsonNodeLoader {

    /// <summary>
    /// Gets the protection scope for the data protection.
    /// </summary>
    public DataProtectionScope? ProtectionScope { get; }

    /// <summary>
    /// Defines the data protection scope for the loader. Null means no data protection used.
    /// </summary>
    /// <param name="protectionScope"></param>
    public JsonNodeProtectedLoader(DataProtectionScope protectionScope) => ProtectionScope = protectionScope;

    /// <summary>
    /// Loads the root node from file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    public override JsonNode Load(string path, bool caseSensitive = false) {
        var isProtected = SettingsProtected.IsProtected(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return Load(stream, caseSensitive, isProtected ? ProtectionScope : null);
    }

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>JSON root node.</returns>
    public override JsonNode Load(Stream stream, bool caseSensitive = false)
        => Load(stream, caseSensitive, ProtectionScope);

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    public override async ValueTask<JsonNode> LoadAsync(string path, bool caseSensitive = false) {
        var isProtected = SettingsProtected.IsProtected(path);
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return await LoadAsync(stream, caseSensitive, isProtected ? ProtectionScope : null);
    }

    /// <summary>
    /// Loads the root node from stream.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    public override ValueTask<JsonNode> LoadAsync(Stream stream, bool caseSensitive = false)
        => LoadAsync(stream, caseSensitive, ProtectionScope);

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public override void Save(JsonNode node, string path) {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        Save(node, stream, ProtectionScope);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public override void Save(JsonNode node, Stream stream)
        => Save(node, stream, ProtectionScope);

    /// <summary>
    /// Saves the current state of the configuration to a file.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    public async override ValueTask SaveAsync(JsonNode node, string path) {
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await SaveAsync(node, stream, ProtectionScope);
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    public override ValueTask SaveAsync(JsonNode node, Stream stream)
        => SaveAsync(node, stream, ProtectionScope);

    #region Data protection implementation

    /// <summary>
    /// Loads the root node from stream using the <see cref="DataProtectionScope"/>.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <param name="protectionScope">Data protection scope or null to load unprotected file.</param>
    /// <returns>JSON root node.</returns>
    private JsonNode Load(Stream stream, bool caseSensitive = false, DataProtectionScope? protectionScope = null) {
        if (protectionScope is null)
            return JsonObject.Create(JsonDocument.Parse(stream, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive)!;
        using var bufferStream = stream.CanSeek ? new MemoryStream(new byte[stream.Length]) : new MemoryStream();
        stream.CopyTo(bufferStream);
        var utf8text = DP.Unprotect(bufferStream.ToArray(), protectionScope.Value);
        return JsonObject.Create(JsonDocument.Parse(utf8text, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive)!;
    }

    /// <summary>
    /// Loads the root node from stream using the <see cref="DataProtectionScope"/>.
    /// </summary>
    /// <param name="stream">JSON stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <param name="protectionScope">Data protection scope or null to load unprotected file.</param>
    /// <returns>A <see cref="ValueTask"/> returning the JSON root node.</returns>
    private async ValueTask<JsonNode> LoadAsync(Stream stream, bool caseSensitive = false, DataProtectionScope? protectionScope = null) {
        if (protectionScope is null)
            return JsonObject.Create(
                (await JsonDocument.ParseAsync(stream, DocumentOptions)).RootElement,
                caseSensitive ? CaseSensitive : CaseInsensitive
            )!;
        await using var bufferStream = stream.CanSeek ? new MemoryStream(new byte[stream.Length]) : new MemoryStream();
        await stream.CopyToAsync(bufferStream);
        var utf8text = DP.Unprotect(bufferStream.ToArray(), protectionScope.Value);
        return JsonObject.Create(
            JsonDocument.Parse(utf8text, DocumentOptions).RootElement, caseSensitive ? CaseSensitive : CaseInsensitive
        )!;
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream using the <see cref="DataProtectionScope"/>.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    /// <param name="protectionScope">Data protection scope or null to save unprotected file.</param>
    private void Save(JsonNode node, Stream stream, DataProtectionScope? protectionScope = null) {
        if (node is null) return;
        if (protectionScope is null) {
            using var writer = new Utf8JsonWriter(stream, WriterOptions);
            node.WriteTo(writer, SerializerOptions);
        }
        else {
            using var plainStream = new MemoryStream();
            using var writer = new Utf8JsonWriter(plainStream, WriterOptions);
            node.WriteTo(writer, SerializerOptions);
            writer.Flush();
            var protectedUtf8Text = DP.Protect(plainStream.ToArray(), protectionScope.Value);
            using var protectedStream = new MemoryStream(protectedUtf8Text);
            protectedStream.CopyTo(stream);
        }
    }

    /// <summary>
    /// Saves the current state of the configuration to a stream.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    /// <param name="protectionScope">Data protection scope or null to save unprotected file.</param>
    private async ValueTask SaveAsync(JsonNode node, Stream stream, DataProtectionScope? protectionScope = null) {
        if (node is null) return;
        if (protectionScope is null) {
            await using var writer = new Utf8JsonWriter(stream, WriterOptions);
            node.WriteTo(writer, SerializerOptions);
        }
        else {
            await using var plainStream = new MemoryStream();
            await using var writer = new Utf8JsonWriter(plainStream, WriterOptions);
            node.WriteTo(writer, SerializerOptions);
            await writer.FlushAsync();
            var protectedUtf8Text = DP.Protect(plainStream.ToArray(), protectionScope.Value);
            await using var protectedStream = new MemoryStream(protectedUtf8Text);
            await protectedStream.CopyToAsync(stream);
        }
    }

    #endregion

}