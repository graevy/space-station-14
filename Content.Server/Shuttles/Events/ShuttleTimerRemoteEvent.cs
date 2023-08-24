using Content.Server.Shuttles.Systems;

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Optionally raised when a Timer queues a <see cref="ShuttleSystem.FasterThanLight"/> action.
/// </summary>
/// <param name="Filter">(delegate<uid1, uid2, bool>, uid2) struct to filter remote screens from an update</param>
[ByRefEvent]
public readonly record struct RemoteShuttleTimerEvent(TimeSpan Countdown, MapFilter? Filter = null);
