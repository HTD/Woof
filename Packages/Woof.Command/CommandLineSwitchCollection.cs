using System.Collections;

namespace Woof.Command;

/// <summary>
/// A collection containing command line switches.
/// </summary>
public sealed class CommandLineSwitchCollection : IEnumerable<string> {

    /// <summary>
    /// Command line switches.
    /// </summary>
    private readonly List<string> Switches;

    /// <summary>
    /// Creates an empty collection.
    /// </summary>
    internal CommandLineSwitchCollection() => Switches = [];

    /// <summary>
    /// Creates new collection from any collection.
    /// </summary>
    /// <param name="values">Initial collection.</param>
    internal CommandLineSwitchCollection(List<string> values) => Switches = values;

    /// <summary>
    /// Returns a value indicating whether the collection contain the switch with specified name or alias.
    /// </summary>
    /// <param name="name">Name of the switch, or name aliases separated with '|' character.</param>
    /// <returns></returns>
    public bool this[string name] => Switches.Any() && (
        name.Contains('|')
        ? name.Split('|').Any(i => Switches.Contains(i, StringComparer.Ordinal))
        : Switches.Contains(name, StringComparer.Ordinal)
    );

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Switches).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)Switches).GetEnumerator();

}
