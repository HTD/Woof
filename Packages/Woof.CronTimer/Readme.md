# Woof.CronTimer

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

A very simple timer / scheduler using the cron expression to define schedules.

### Features

It was created not to use big schedulers and to make the scheduler completely
detached from the scheduled jobs.

It is used like most of the system timers, however instead of interval at least
one `ScheduledEvent` is needed to make it raise the `Impulse` events.

The scheduler uses `NCrontab` library for parsing `Cron` expressions.
The library is capable of parsing 5 and 6 fields expressions with following 
formats:

- `[minute]` `[hour]` `[day of the month]` `[month]` `[day of the week]`
- `[second]` `[minute]` `[hour]` `[day of the month]` `[month]` `[day of the week]`

The correct format is detected automatically while creating
the `ScheduledEvent`.

As the scheduler behaves like most .NET timers, it doesn't use a timer itself.
It creates a one long running task awaiting the scheduled events and by default
it creates a new task for each event handler to prevent it from blocking
the main thread.

If the event handler is asynchronous and it can be guaranteed that it won't
block the main thread, `IsImpulseHandledInNewTask` property can be set to false
to skip creating new tasks for event handlers. This is an optional performance
optimization feture.

The other possible optimization is decreasing the timer resolution
by increasing the `TimeSpan` assigned to the `Resolution` property. It checks
the schedule every second that allows extended, 1 second precission expressions
to be handled on time. If such precision is not needed, the time can
be increased to lower the power consumption.

The `CronTimer` class requires a type attribute to specify what type of data
will be bound to a scheduled event. It determines the type of the `Data`
property of the `ScheduledEvent` and `Impulse` event arguments.

When the `Impulse` event is raised, the assigned data is available in its
arguments parameter.

The event list can be modified at runtime ONLY with the
`AddEventAsync()` and `RemoveEventsAsync()` methods.
Modifying the list items directly will crash the scheduler.

Those methods should be called only asynchronously.
Synchronous calls will obviously deadlock the scheduler.

## Usage

1. Create `Woof.Cron.CronTimer<T>` instance.
2. Add an `Impulse` event handler.
3. Add at least one `ScheduledEvent` to `Events` property.
4. Start the timer with the `Start` method.
5. The scheduler can be stopped with `Stop` or `Dispose` method.
6. The `Dispose` method just stops the timer if it's started to dispose
   its task and cancellation token source.

Check the demo on GitHub for more details.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.