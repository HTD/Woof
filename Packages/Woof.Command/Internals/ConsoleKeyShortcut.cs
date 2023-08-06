namespace Woof.Command.Internals;

/// <summary>
/// Structure containing user defined console key shortcut.
/// LATER: Test using <see cref="IsMatch(ConsoleKeyInfo)"/> method.
/// </summary>
internal readonly struct ConsoleKeyShortcut {

    /// <summary>
    /// Gets the key part of the shortcut.
    /// </summary>
    public ConsoleKey Key { get; }

    /// <summary>
    /// Gets the modifiers part of the shortcut.
    /// </summary>
    public ConsoleModifiers Modifiers { get; }

    /// <summary>
    /// Creates new instance of the shortcut.
    /// </summary>
    /// <param name="modifiers">The modifiers, 0 for no modifiers.</param>
    /// <param name="key">The key.</param>
    public ConsoleKeyShortcut(ConsoleModifiers modifiers, ConsoleKey key) {
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    /// Tests if the pressed keyboard chord matches the defined shortcut.
    /// </summary>
    /// <param name="k">Key and modifiers.</param>
    /// <returns>True if key and modifiers were exactly as defined.</returns>
    public bool IsMatch(ConsoleKeyInfo k) => k.Modifiers == Modifiers && k.Key == Key;

}
