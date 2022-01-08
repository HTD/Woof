namespace Woof.Windows.Demo.Models;

/// <summary>
/// Allows introducins small very inaccurate lags less than 1ms.
/// </summary>
public static class Lag {

    /// <summary>
    /// Spin-waits for a specified number of milliseconds.<br/>
    /// The smaller the time or the slower is the executing machine, the less accurate result.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to wait. Limited with machine speed and system load.</param>
    public static void Wait(double milliseconds) {
        Stopwatch? stopwatch = new();
        long tickCount = (long)(milliseconds * 10000L);
        stopwatch.Start();
        while (stopwatch.ElapsedTicks < tickCount) ;
    }

    /// <summary>
    /// Returns the task that spin-waits for a specified number of milliseconds.<br/>
    /// The smaller the time or the slower is the executing machine, the less accurate result.<br/>
    /// Creating and using the task will take considerable amount of additional time.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to wait. Limited with machine speed and system load.</param>
    public static Task WaitAsync(double milliseconds) => Task.Run(() => Wait(milliseconds));

}