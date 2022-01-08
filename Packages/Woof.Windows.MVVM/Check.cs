namespace Woof.Windows.Mvvm;

/// <summary>
/// The <see cref="ICheckableValue"/> implementation that supports sorting.
/// </summary>
public class Check : ICheckableValue, IComparable, IComparable<ICheckableValue> {

    /// <summary>
    /// Gets or sets a value indicating that the object is checked.
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Gets or sets the value of the object.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Creates new <see cref="ICheckable"/> wrapper on any object.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="isChecked">True if the object is checked.</param>
    public Check(object? value, bool isChecked = false) {
        Value = value;
        IsChecked = isChecked;
    }

    /// <summary>
    /// Gets the string representation of the <see cref="Value"/>, result depends on the specific object's ToString() implementation.
    /// </summary>
    /// <returns>String representation of the <see cref="Value"/> or null if the <see cref="Value"/> is null.</returns>
    public override string? ToString() => Value?.ToString();

    /// <summary>
    /// Compares one <see cref="Check"/> item with the other.
    /// </summary>
    /// <param name="other">The other check item.</param>
    /// <returns>-1 if this &gt; other, 0 if equal, 1 if this &lt; other.</returns>
    public int CompareTo(ICheckableValue? other)
        => Value is IComparable comparable
            ? comparable.CompareTo(other?.Value)
            : String.Compare(ToString(), other?.ToString());

    /// <summary>
    /// Compares one <see cref="Check"/> item with the other.
    /// </summary>
    /// <param name="obj">The other check item.</param>
    /// <returns>-1 if this &gt; other, 0 if equal, 1 if this &lt; other.</returns>
    public int CompareTo(object? obj)
        => Value is IComparable comparable
            ? comparable.CompareTo((obj as IValue)?.Value)
            : String.Compare(ToString(), obj?.ToString());

}