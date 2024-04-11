using Robust.Shared.Serialization;

namespace Content.Shared.TextScreen;

[Serializable, NetSerializable]
public enum TextScreenVisuals : byte
{
    // TODO: support for a small image, I think. Probably want to rename textscreen to just screen then.
    /// <summary>
    ///     What text to default to after timer completion?
    ///     Expects a <see cref="string"/>.
    /// </summary>
    DefaultText,
    /// <summary>
    ///     What text to render? <br/>
    ///     Expects a <see cref="string"/>.
    /// </summary>
    ScreenText,

    /// <summary>
    ///     What is the target time? <br/>
    ///     Expects a <see cref="TimeSpan"/>.
    /// </summary>
    TargetTime,

    /// <summary>
    ///     Change text color on the entire screen
    ///     Expects a <see cref="Color"/>.
    /// </summary>
    Color
}

public sealed class TextScreenColor
{
    // 15,151,251 is the old ss13 screen color from tg
    public static readonly Color TGBlue = new Color(15, 151, 251);
};
