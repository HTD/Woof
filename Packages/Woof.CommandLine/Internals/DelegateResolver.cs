namespace Woof.Internals.CommandLine;

/// <summary>
/// Allows calling the delegates of certain types.
/// </summary>
static class DelegateResolver {

    /// <summary>
    /// Calls the delegate asynchronously
    /// if the delegate is a synchronous or asynchronous method that returns nothing and accepts nothing.<br/>
    /// If the delegate accepts non nullable value and no value is provided, it is not called.
    /// </summary>
    /// <param name="d">Delegate to call.</param>
    /// <returns>Task completed when the delegate call completes.</returns>
    public static async Task CallAsync(this Delegate d) {
        if (d is Action sync) sync();
        else if (d is Func<Task> async) await async();
#if NET5_0_OR_GREATER
        else if (d is Func<ValueTask> vAsync) await vAsync();
#endif
    }

    /// <summary>
    /// Calls the delegate asynchronously
    /// if the delegate is a synchronous or asynchronous method that returns nothing and accepts string, int or double.<br/>
    /// If the delegate accepts non nullable value and no value is provided, it is not called.
    /// </summary>
    /// <param name="d">Delegate to call.</param>
    /// <param name="value">Optional value to pass.</param>
    /// <returns>Task completed when the delegate call completes.</returns>
    public static async Task CallAsync(this Delegate d, string? value) {
        if (d is Action<string?> syncString) syncString(value);
        else if (d is Action<int> syncInt && value is not null) syncInt(int.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Action<int?> syncIntNull) syncIntNull(value is null ? null : int.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Action<double> syncDouble && value is not null) syncDouble(double.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Action<double?> syncDoubleNull) syncDoubleNull(value is null ? null : double.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<string?, Task> asyncString) await asyncString(value);
        else if (d is Func<int, Task> asyncInt && value is not null) await asyncInt(int.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<int?, Task> asyncIntNull) await asyncIntNull(value is null ? null : int.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<double, Task> asyncDouble && value is not null) await asyncDouble(double.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<double?, Task> asyncDoubleNull) await asyncDoubleNull(value is null ? null : double.Parse(value, CultureInfo.InvariantCulture));
#if NET5_0_OR_GREATER
        else if (d is Func<string?, ValueTask> vAsyncString) await vAsyncString(value);
        else if (d is Func<int, ValueTask> vAsyncInt && value is not null) await vAsyncInt(int.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<int?, ValueTask> vAsyncIntNull) await vAsyncIntNull(value is null ? null : int.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<double, ValueTask> vAsyncDouble && value is not null) await vAsyncDouble(double.Parse(value, CultureInfo.InvariantCulture));
        else if (d is Func<double?, ValueTask> vAsyncDoubleNull) await vAsyncDoubleNull(value is null ? null : double.Parse(value, CultureInfo.InvariantCulture));
#endif
    }

}
