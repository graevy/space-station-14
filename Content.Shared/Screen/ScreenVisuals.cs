using Robust.Shared.Serialization;

namespace Content.Shared.Screen;

[Serializable, NetSerializable]
public enum ScreenVisuals : byte
{
    Update
    // TODO: support for a small image, I think.
}

[Serializable, NetSerializable]
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
[Serializable, NetSerializable]
public sealed class ScreenMasks
{
    // main updates dict
    public static readonly string Updates = Loc.GetString("screen-updates");

    // shuttle timer accompanying text
    public static readonly string ETA = Loc.GetString("screen-eta");
    public static readonly string ETD = Loc.GetString("screen-etd");
    public static readonly string Bye = Loc.GetString("screen-bye");
    public static readonly string Kill = Loc.GetString("screen-kill");

    // nuke timer accompanying text
    public static readonly string Nuke = Loc.GetString("screen-nuke");
}

[Serializable, NetSerializable]
public struct ScreenUpdate
{
    public NetEntity? Subnet { get; }
    public ScreenPriority Priority { get; }
    public string? Text { get; }
    public TimeSpan? Timer { get; }
    public Color? Color { get; }

    public ScreenUpdate(NetEntity? subnet, ScreenPriority priority, string? text = null, TimeSpan? timer = null, Color? color = null)
    {
        Subnet = subnet; Priority = priority; Text = text; Timer = timer; Color = color;
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
