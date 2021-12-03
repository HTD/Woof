namespace Woof.Net;

/// <summary>
/// Retries operations using provided time pattern.
/// </summary>
public class OperationRetrier {

    /// <summary>
    /// Creates a new instance with specified retry times.
    /// </summary>
    /// <param name="times">Retry times in seconds between each attempt.</param>
    public OperationRetrier(params double[] times) {
        if (times.Length < 1) throw new ArgumentException("At least one time value is required", nameof(times));
        Times = times.Select(i => TimeSpan.FromSeconds(i)).ToArray();
    }

    /// <summary>
    /// Checks if the retrying is enabled and waiting for the next attempt is applicable.
    /// If false is returned the caller should quit.
    /// </summary>
    /// <returns>True if the caller should wait and retry the operation.</returns>
    public bool Check() {
        if (TimeIndex >= Times.Length) {
            TimeIndex = 0;
            return IsWaitEnabled = false;
        }
        CurrentWait = Times[TimeIndex];
        TimeIndex++;
        return IsWaitEnabled = true;
    }

    /// <summary>
    /// Resets the retrying counter on successful operation.
    /// </summary>
    public void Reset() {
        TimeIndex = 0;
        CurrentWait = Times[0];
    }

    /// <summary>
    /// Waits before retrying the operations.
    /// </summary>
    /// <returns>Task completed when the configured amount of time passed.</returns>
    public async Task WaitAsync() {
        if (IsWaitEnabled) await Task.Delay(CurrentWait);
        else throw new InvalidOperationException("Retries count exceeded / not checked");
    }

    #region Private data

    readonly TimeSpan[] Times;
    int TimeIndex;
    bool IsWaitEnabled;
    TimeSpan CurrentWait;

    #endregion

}
