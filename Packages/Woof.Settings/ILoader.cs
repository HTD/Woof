namespace Woof.Settings;

/// <summary>
/// Defines methods for parsing, loading and saving <typeparamref name="T"/> type.
/// </summary>
/// <typeparam name="T">The type of the serialized data container.</typeparam>
public interface ILoader<T> {

    /// <summary>
    /// Parses the serialized text.
    /// </summary>
    /// <param name="text">Serialized text.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>Root node.</returns>
    T Parse(string text, bool caseSensitive = false);

    /// <summary>
    /// Loads a new <typeparamref name="T"/> from a serialized file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns><typeparamref name="T"/> instance.</returns>
    T Load(string path, bool caseSensitive = false);

    /// <summary>
    /// Loads a new <typeparamref name="T"/> from a serialized stream.
    /// </summary>
    /// <param name="stream">Serialized stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns><typeparamref name="T"/> instance.</returns>
    T Load(Stream stream, bool caseSensitive = false);

    /// <summary>
    /// Loads a new <typeparamref name="T"/> from a serialized file.
    /// </summary>
    /// <param name="path">A path to the file.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <typeparamref name="T"/> instance.</returns>
    ValueTask<T> LoadAsync(string path, bool caseSensitive = false);

    /// <summary>
    /// Creates a new <typeparamref name="T"/> from a stream.
    /// </summary>
    /// <param name="stream">serialized stream.</param>
    /// <param name="caseSensitive">Case sensitive property matching.</param>
    /// <returns>A <see cref="ValueTask"/> returning <typeparamref name="T"/> instance.</returns>
    ValueTask<T> LoadAsync(Stream stream, bool caseSensitive = false);

    /// <summary>
    /// Saves the current state of the <typeparamref name="T"/> to a file.
    /// </summary>
    /// <param name="node">The <typeparamref name="T"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    void Save(T node, string path);

    /// <summary>
    /// Saves the current state of the <typeparamref name="T"/> to a stream.
    /// </summary>
    /// <param name="node">The <typeparamref name="T"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    void Save(T node, Stream stream);

    /// <summary>
    /// Saves the current state of the <typeparamref name="T"/> to a file.
    /// </summary>
    /// <param name="node">The <typeparamref name="T"/> to save.</param>
    /// <param name="path">A path to the file.</param>
    ValueTask SaveAsync(T node, string path);

    /// <summary>
    /// Saves the current state of the <typeparamref name="T"/> to a stream.
    /// </summary>
    /// <param name="node">The <typeparamref name="T"/> to save.</param>
    /// <param name="stream">Writeable stream.</param>
    ValueTask SaveAsync(T node, Stream stream);

}