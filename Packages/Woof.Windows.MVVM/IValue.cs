namespace Woof.Windows.Mvvm;

/// <summary>
/// Describes an object that has a value object.
/// </summary>
public interface IValue {

    /// <summary>
    /// Gets or sets the object's value.
    /// </summary>
    object? Value { get; set; }

}