// using System.Collections.Generic;

using Content.Shared.Screen;

namespace Content.Server.Screens.Components;

public abstract class ScreenComparer : IComparer<ScreenUpdate>
{
    public ScreenComparer()
    {

    }
    public int Compare(ScreenUpdate a, ScreenUpdate b)
    {
        if (a.Priority >= b.Priority)
            return 0;
        else
            return 1;
    }
}

public abstract class ScreenComparer : IComparer<ScreenUpdate>
{
    public ScreenComparer()
    {
    }
    public int Compare(ScreenUpdate a, ScreenUpdate b)
    {
        if (a.Priority > b.Priority)
            return 1;
        else if (a.Priority < b.Priority)
            return -1;
        else
            return 0;
    }
}

[RegisterComponent]
public sealed partial class ScreenComponent : Component
{
    public static readonly uint MaxUpdates = 16;
    public ScreenUpdate? ActiveUpdate;
    public SortedSet<ScreenUpdate> Updates;

}

public struct ScreenUpdate
{
    public EntityUid? Subnet { get; }
    public int Priority { get; }
    public string? Text { get; }
    public TimeSpan? Timer { get; }
    public Color? Color { get; }

    public ScreenUpdate(EntityUid? subnet, int priority, string? text = null, TimeSpan? timer = null, Color? color = null)
    {
        Subnet = subnet; Priority = priority; Text = text; Timer = timer; Color = color;
    }
}

/// <summary>
///     Player-facing hashable consts for NetworkPayloads
/// </summary>
public sealed class ScreenMasks
{
    public static readonly string Text = Loc.GetString("screen-text");
    public static readonly string Timer = Loc.GetString("screen-timer");
    public static readonly string Color = Loc.GetString("screen-color");
    public static readonly string Priority = Loc.GetString("screen-priority");

    // higher priority updates display above lower priority updates
    public static readonly int DefaultPriority = 0;
    public static readonly int ShuttlePriority = 1;
    public static readonly int NukePriority = 2;

    // // if you want to use these hardcoded freqs, you probably need to add DeviceNetworkComponent to something instead
    // public static readonly uint MainFreq = 2450;
    // public static readonly uint ArrivalsFreq = 2451;

    // main updates dict
    public static readonly string Updates = Loc.GetString("screen-timer-updates");

    //     public static readonly string ShuttleTime = Loc.GetString("shuttle-timer-shuttle-time");
    //     public static readonly string DestTime = Loc.GetString("shuttle-timer-dest-time");
    //     public static readonly string SourceTime = Loc.GetString("shuttle-timer-source-time");
    //     public static readonly string ShuttleMap = Loc.GetString("shuttle-timer-shuttle-map");
    //     public static readonly string SourceMap = Loc.GetString("shuttle-timer-source-map");
    //     public static readonly string DestMap = Loc.GetString("shuttle-timer-dest-map");
    //     public static readonly string Docked = Loc.GetString("shuttle-timer-docked");

    // shuttle timers
    public static readonly string ETA = Loc.GetString("screen-timer-eta");
    public static readonly string ETD = Loc.GetString("screen-timer-etd");
    public static readonly string Bye = Loc.GetString("screen-timer-bye");
    public static readonly string Kill = Loc.GetString("screen-timer-kill");

    // nuke timers
    public static readonly string Nuke = Loc.GetString("screen-timer-nuke");
}
