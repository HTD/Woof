using System.Collections;

namespace Woof.Command;

/// <summary>
/// A collection containing positional command line arguments.
/// Can be queried without range checking.
/// </summary>
public sealed class CommandLinePositionalArgumentCollection : IEnumerable<string> {

    /// <summary>
    /// Command line arguments.
    /// </summary>
    private readonly List<string> Arguments;

    /// <summary>
    /// Creates an empty collection.
    /// </summary>
    internal CommandLinePositionalArgumentCollection() => Arguments = new(0);

    /// <summary>
    /// Creates new collection from any collection.
    /// </summary>
    /// <param name="values">Initial collection.</param>
    internal CommandLinePositionalArgumentCollection(List<string> values) => Arguments = values;

    /// <summary>
    /// Returns the command line argument selected by the index or null if such element doesn't exist.
    /// </summary>
    /// <param name="index">Zero based index of the positional argument.</param>
    /// <returns>Argument value or null.</returns>
    public string? this[int index] => index >= 0 && index < Arguments.Count ? Arguments[index] : null;

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<string> GetEnumerator() => Arguments.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => Arguments.GetEnumerator();

}
