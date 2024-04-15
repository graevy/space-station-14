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

// public sealed class ScreenUpdates
// {
//     // forgive me
//     public Dictionary<int, Stack<ScreenUpdate>> Priorities = new();
//     public uint Count = 0;

//     public ScreenUpdates()
//     {
//     }

//     public void Append(ScreenUpdate update)
//     {
//         if (!Priorities.ContainsKey(update.Priority))
//             Priorities.Add(update.Priority, new Stack<ScreenUpdate>());

//         var stack = Priorities[update.Priority];

//         // if an update doesn't have a timer, it never expires, so everything else in the stack can be safely removed
//         if (!update.HasTimer())
//             stack.Clear();

//         stack.Push(update);
//     }

//     public void Remove()
//     {

//     }
// }