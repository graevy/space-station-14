using Content.Server.Shuttles.Systems;

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Optionally raised when a Timer queues a <see cref="ShuttleSystem.FasterThanLight"/> action.
/// </summary>
[ByRefEvent]
public readonly record struct AllShuttleTimerEvent(TimeSpan Countdown);
