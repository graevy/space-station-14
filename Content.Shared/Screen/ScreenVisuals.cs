using Robust.Shared.Serialization;

namespace Content.Shared.Screen;

[Serializable, NetSerializable]
public enum ScreenVisuals : byte
{
    Update
    // TODO: support for a small image, I think. Probably want to rename textscreen to just screen then.
    /// <summary>
    ///     What text to default to after timer completion?
    ///     Expects a <see cref="string"/>.
    /// </summary>
    // DefaultText,
    // /// <summary>
    // ///     What text to render? <br/>
    // ///     Expects a <see cref="string"/>.
    // /// </summary>
    // ScreenText,

    // /// <summary>
    // ///     What is the target time? <br/>
    // ///     Expects a <see cref="TimeSpan"/>.
    // /// </summary>
    // TargetTime,

    // /// <summary>
    // ///     Change text color on the entire screen
    // ///     Expects a <see cref="Color"/>.
    // /// </summary>
    // Color
}

public enum ScreenPriority : byte
{
    Nuke,
    Brig,
    Shuttle,
    Default
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

    // lower priority int = higher priority display
    // public static readonly int DefaultPriority = 40;
    // public static readonly int ShuttlePriority = 30;
    // public static readonly int BrigPriority = 20;
    // public static readonly int NukePriority = 10;

    // main updates dict
    public static readonly string Updates = Loc.GetString("screen-timer-updates");

    // shuttle timers
    public static readonly string ETA = Loc.GetString("screen-timer-eta");
    public static readonly string ETD = Loc.GetString("screen-timer-etd");
    public static readonly string Bye = Loc.GetString("screen-timer-bye");
    public static readonly string Kill = Loc.GetString("screen-timer-kill");

    // nuke timers
    public static readonly string Nuke = Loc.GetString("screen-timer-nuke");
}

public struct ScreenUpdate
{
    public EntityUid? Subnet { get; }
    public ScreenPriority Priority { get; }
    public string? Text { get; }
    public TimeSpan? Timer { get; }
    public Color? Color { get; }

    public ScreenUpdate(EntityUid? subnet, ScreenPriority priority, string? text = null, TimeSpan? timer = null, Color? color = null)
    {
        Subnet = subnet; Priority = priority; Text = text; Timer = timer; Color = color;
    }

    public static bool operator ==(ScreenUpdate left, ScreenUpdate? right)
    {
        if (right is null)
            return false;

        return left == right.Value;
    }

    public static bool operator !=(ScreenUpdate left, ScreenUpdate? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is ScreenUpdate))
            return false;

        ScreenUpdate other = (ScreenUpdate) obj;
        return Subnet == other.Subnet && Priority == other.Priority && Text == other.Text && Timer == other.Timer && Color == other.Color;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Subnet, Priority, Text, Timer, Color);
    }

    public bool HasTimer()
    {
        return Timer != null;
    }

}
