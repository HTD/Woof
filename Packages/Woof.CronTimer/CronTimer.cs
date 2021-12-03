namespace Woof.Cron;

/// <summary>
/// A very simple task scheduler using cron expressions to schedule events.<br/>
/// 5 fields format: [minute] [hour] [day of the month] [month] [day of the week].<br/>
/// 6 fields format: [second] [minute] [hour] [day of the month] [month] [day of the week].<br/>
/// The [year] field is NOT SUPPORTED.<br/><br/>
/// Use <see cref="ScheduledEvent.Data"/> to pass additional information about the job to be done on the schedule.
/// </summary>
/// <typeparam name="TData">A type of data sent with the <see cref="Impulse"/> event.</typeparam>
/// <remarks>
/// The idea is to completely decouple the jobs from the scheduler.
/// Uses <see cref="NCrontab.CrontabSchedule"/> to parse cron expressions.<br/>
/// </remarks>
public sealed partial class CronTimer<TData> : IDisposable {

    /// <summary>
    /// Gets the list of sheduled events.
    /// </summary>
    public List<ScheduledEvent> Events { get; } = new();

    /// <summary>
    /// Gets or sets the timer resolution. Default set to 1 second.
    /// It makes sense to increase it for power saving when 1 second resolution is not necessary.
    /// </summary>
    public TimeSpan Resolution { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that the impulse event will be called in a new task.<br/>
    /// True is default, safe choice, so the event handler can't block the clock thread.<br/>
    /// Set false for fully asynchronous event handlers, to save the cost of starting a new task on event handler invocation.<br/>
    /// Also when this property is set to false the coincident events will be called in the same order they were added.
    /// </summary>
    public bool IsImpulseHandledInNewTask { get; set; } = true;

    /// <summary>
    /// Gets a value indicating that the timer is started.
    /// </summary>
    public bool IsStarted => CTS is not null;

    /// <summary>
    /// Occures when it's time for the scheduled event.
    /// </summary>
    public event EventHandler<TData>? Impulse;

    /// <summary>
    /// Creates an instance of the scheduler.
    /// </summary>
    /// <param name="resolution">Timer resolution, default 1 second.</param>
    public CronTimer(TimeSpan resolution = default)
        => Resolution = resolution == default ? TimeSpan.FromSeconds(1) : resolution;

    /// <summary>
    /// Starts the scheduler timer if not started.
    /// </summary>
    public void Start() {
        if (CTS != null) return;
        CTS = new();
        var token = CTS.Token;
        Task.Factory.StartNew(async () => await MainLoop(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary>
    /// Stops the scheduler timer if started.
    /// </summary>
    public void Stop() {
        if (CTS is null) return;
        CTS.Cancel();
        CTS.Dispose();
        CTS = null;
    }

    /// <summary>
    /// Main loop used to check the scheduled events and invoke <see cref="Impulse"/> event when it is its time.
    /// </summary>
    /// <param name="token">Cancellation token used to break the loop.</param>
    /// <returns>Task completed when the cancellation token is canceled.</returns>
    async Task MainLoop(CancellationToken token = default) {
        while (!token.IsCancellationRequested) {
            var now = DateTime.Now;
            var @base = now - Resolution;
            await EventsLock.WaitAsync(token);
            foreach (var @event in Events) {
                var due = @event.Schedule.GetNextOccurrence(@base);
                if (now >= due && Impulse is not null) {
                    if (IsImpulseHandledInNewTask) _ = Task.Run(() => Impulse(this, @event.Data), token);
                    else Impulse(this, @event.Data);
                }
            }
            EventsLock.Release();
            await Task.Delay(Resolution, token);
        }
    }

    /// <summary>
    /// Adds a scheduled event to the current event list on the next timer tick.
    /// </summary>
    /// <param name="item">An event to add.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the events list is modified.</returns>
    public async ValueTask AddEventAsync(ScheduledEvent item) {
        await EventsLock.WaitAsync();
        Events.Add(item);
        EventsLock.Release();

    }

    /// <summary>
    /// Removes events matching the predicate on the next timer tick.
    /// </summary>
    /// <param name="predicate">Predicate matching the data to remove.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the events list is modified.</returns>
    public async ValueTask RemoveEventsAsync(Func<TData, bool> predicate) {
        await EventsLock.WaitAsync();
        while (Events.FirstOrDefault(e => predicate(e.Data)) is ScheduledEvent target)
            Events.Remove(target);
        EventsLock.Release();
    }

    /// <summary>
    /// Ensures the cancellation token source used to start and stop the instance is disposed.<br/>
    /// When the instance is stopped it does nothing.
    /// </summary>
    public void Dispose() => Stop();

    private CancellationTokenSource? CTS;
    private readonly SemaphoreSlim EventsLock = new(1, 1);
    private static readonly Regex RxItems = new(@"\s*,\s*", RegexOptions.Compiled);
    private static readonly Regex RxFields = new(@"\s+", RegexOptions.Compiled);

}
