namespace Woof.Net;

/// <summary>
/// Asynchronous loop runner.
/// </summary>
public static class AsyncLoop {

    /// <summary>
    /// Creates a new task with an asynchronous loop.
    /// </summary>
    /// <param name="iteration">A delegate called on each loop iteration.</param>
    /// <param name="exceptionHandler">Exception handler delegate for the iteration.</param>
    /// <param name="condition">Optional condition that must evaluate true for the loop to continue or start.</param>
    /// <param name="breakOnException">If set true, exceptions in iteration should break the loop.</param>
    /// <param name="token">Cancellation token used to end the loop.</param>
    /// <returns>The started <see cref="Task{TResult}"/>.</returns>
    public static Task FromIterationAsync(
        Delegate? iteration,
        Delegate? exceptionHandler = null,
        Func<bool>? condition = null,
        bool breakOnException = false,
        CancellationToken token = default
    ) => Task.Factory.StartNew(async () => {
        try {
            while (!token.IsCancellationRequested && (condition is null || condition())) {
                try {
                    if (iteration is Action syncIteration) syncIteration();
                    else if (iteration is Func<ValueTask> asyncIterationV) await asyncIterationV();
                    else if (iteration is Func<Task> asyncIteration) await asyncIteration();
                }
                catch (Exception exception) {
                    if (exceptionHandler is Action<Exception> syncXH) syncXH(exception);
                    else if (exceptionHandler is Func<Exception, ValueTask> asyncXHV) await asyncXHV(exception);
                    else if (exceptionHandler is Func<Exception, Task> asyncXH) await asyncXH(exception);
                    if (breakOnException) break;
                }
            }
        }
        catch (TaskCanceledException) { }
    }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

}