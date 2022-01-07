using System;
using System.Linq;
using Woof.Cron;

namespace Woof.CronTimer.Demo;

/// <summary>
/// A live test for the scheduler.
/// </summary>
class Program {

    /// <summary>
    /// Creates some data and schedules, then runs the timer and displays information about raised events.
    /// </summary>
    static void Main() {
        var sources = new SchedulerData[] {
                new(1, "A"),
                new(1, "B"),
                new(2, "C")
            };
        var events = new CronTimer<SchedulerData>.ScheduledEvent[] {
                new("* * * * * *", sources[0]),
                new("*/2 * * * * *", sources[1]),
                new("*/3 * * * * *", sources[2])
            };
        var scheduler = new CronTimer<SchedulerData> { IsImpulseHandledInNewTask = false };
        scheduler.Events.AddRange(events);
        scheduler.Impulse += Scheduler_Impulse; ;
        scheduler.Start();
        Console.WriteLine("Scheduler started, press any key to stop...");
        Console.ReadKey(intercept: true);
        scheduler.Stop();
        Console.WriteLine("Scheduler stopped, press any key to dispose...");
        Console.ReadKey(intercept: true);
        scheduler.Dispose();
        Console.WriteLine("Scheduler disposed, press any key to exit...");
        Console.ReadKey(intercept: true);
    }

    /// <summary>
    /// Impulse event target. Just displays what it got from the scheduler.
    /// ALSO: MODIFIES THE EVENTS LIST AFTER the third B!
    /// </summary>
    /// <param name="sender">Scheduler.</param>
    /// <param name="e">Data.</param>
    private static async void Scheduler_Impulse(object? sender, SchedulerData e) {
        var now = DateTime.Now;
        var scheduler = (sender as CronTimer<SchedulerData>)!;
        Console.WriteLine($"### Time: {now:HH:mm:ss}, SourceId: {e.SourceId}, HandlerId: {e.HandlerId}");
        try {
            if (e.HandlerId == "B") BCounter++;
            if (!IsMod1Done && BCounter > 2) {
                IsMod1Done = true;
                var eventA = scheduler.Events.FirstOrDefault(e => e.Data.HandlerId == "A");
                if (eventA is not null) eventA.Expression = "*/4 * * * * *";

                await scheduler.RemoveEventsAsync(d => d.HandlerId == "B");
                await scheduler.AddEventAsync(new("* * * * * *", new(1, "D")));
            }
        } catch (Exception x) {
            Console.Error.WriteLine(x.Message);
        }
    }

    static int BCounter;
    static bool IsMod1Done;

}

/// <summary>
/// Test scheduler data to determine what source wants to use what handler.
/// </summary>
class SchedulerData {

    /// <summary>
    /// Gets the source identifier.
    /// </summary>
    public int SourceId { get; }

    /// <summary>
    /// Gets the handler identifier.
    /// </summary>
    public string HandlerId { get; }

    /// <summary>
    /// Creates new scheduler data instance.
    /// </summary>
    /// <param name="sourceId">Source identifier.</param>
    /// <param name="handlerId">Handler identifier.</param>
    public SchedulerData(int sourceId, string handlerId) {
        SourceId = sourceId;
        HandlerId = handlerId;
    }

}
