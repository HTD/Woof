namespace Woof.Windows.Mvvm;

/// <summary>
/// Describes an object that can be checked or unchecked.
/// </summary>
public interface ICheckable {

    /// <summary>
    /// Gets or sets a value indicating the object is checked.
    /// </summary>
    bool IsChecked { get; set; }

}