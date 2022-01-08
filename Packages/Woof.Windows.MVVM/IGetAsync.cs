namespace Woof.Windows.Mvvm;

/// <summary>
/// Class implementing this interface must have GetAsync method and IsLoaded property.
/// It's purpose is to provide automatic view model loading.
/// </summary>
public interface IGetAsync {

    /// <summary>
    /// Gets the value indicating the data for the view model has been loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets the data for the view model asynchronously.
    /// </summary>
    /// <returns>Task.</returns>
    ValueTask GetAsync();

}
