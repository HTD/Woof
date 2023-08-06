using System.Collections;

namespace Woof.Command;

/// <summary>
/// A collection containing command line arguments accessible by name or alias.
/// </summary>
public sealed class CommandLineOptionCollection : IEnumerable<KeyValuePair<string, string>> {

    /// <summary>
    /// Command line options.
    /// </summary>
    private readonly Dictionary<string, string> Options;

    /// <summary>
    /// Creates an empty collection.
    /// </summary>
    internal CommandLineOptionCollection() => Options = new Dictionary<string, string>(0);

    /// <summary>
    /// Creates new collection from a dictionary.
    /// </summary>
    /// <param name="values">Initial dictionary.</param>
    internal CommandLineOptionCollection(Dictionary<string, string> values) => Options = values;

    /// <summary>
    /// Gets the option value with the specified name or alias in the <see cref="CommandLineOptionCollection"/>.
    /// </summary>
    /// <param name="name">Name of the option, or name aliases separated with '|' character.</param>
    /// <returns>Option value, null if it doesn't exist.</returns>
    public string? this[string name] => !Options.Any() ? null : (
            name.Contains('|')
                ? name.Split('|').Select(i => Options.TryGetValue(i, out var v) ? v : null).FirstOrDefault(i => i is not null)
                : Options.TryGetValue(name, out var v) ? v : null
        );

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)Options).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)Options).GetEnumerator();

}
