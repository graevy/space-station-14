using System.Linq;
using System.Numerics;
using Content.Shared.Screen;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Screen;

/// overview:
/// Data is passed from server to client through <see cref="SharedAppearanceSystem.SetData"/>,
/// calling <see cref="OnAppearanceChange"/>, which calls almost everything else.

/// Data for the (at most one) timer is stored in <see cref="ScreenTimerComponent"/>.

/// All screens have <see cref="ScreenVisualsComponent"/>, but:
/// the update method only updates the timers, so the timercomp is added/removed by appearance changes/timing out.

/// Because the sprite component stores layers in a dict with no nesting, individual layers
/// have to be mapped to unique ids e.g. {"textMapKey01" : <first row, second char layerstate>}
/// in either the visuals or timer component.


/// <summary>
///     The ScreenSystem draws text in the game world using 3x5 sprite states for each character.
/// </summary>
public sealed class ScreenSystem : VisualizerSystem<ScreenVisualsComponent>
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    /// <summary>
    ///     Contains char/state Key/Value pairs. <br/>
    ///     The states in Textures/Effects/text.rsi that special character should be replaced with.
    /// </summary>
    private static readonly Dictionary<char, string> CharStatePairs = new()
        {
            { ':', "colon" },
            { '!', "exclamation" },
            { '?', "question" },
            { '*', "star" },
            { '+', "plus" },
            { '-', "dash" },
            { ' ', "blank" }
        };

    private const string DefaultState = "blank";

    /// <summary>
    ///     A string prefix for all text layers.
    /// </summary>
    private const string TextMapKey = "textMapKey";
    /// <summary>
    ///     A string prefix for all timer layers.
    /// </summary>
    private const string TimerMapKey = "timerMapKey";
    private const string TextPath = "Effects/text.rsi";
    private const int CharWidth = 4;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreenVisualsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ScreenTimerComponent, ComponentInit>(OnTimerInit);

        UpdatesOutsidePrediction = true;
    }

    private void OnInit(EntityUid uid, ScreenVisualsComponent component, ComponentInit args)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        // awkward to specify a textoffset of e.g. 0.1875 in the prototype
        component.TextOffset = Vector2.Multiply(ScreenVisualsComponent.PixelSize, component.TextOffset);
        component.TimerOffset = Vector2.Multiply(ScreenVisualsComponent.PixelSize, component.TimerOffset);

        ClearScreen(uid, component, sprite);
    }

    /// <summary>
    ///     Instantiates <see cref="SpriteComponent.Layers"/> with {<see cref="TimerMapKey"/> + int : <see cref="DefaultState"/>} pairs.
    /// </summary>
    private void OnTimerInit(EntityUid uid, ScreenTimerComponent timer, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || !TryComp<ScreenVisualsComponent>(uid, out var screen))
            return;

        for (var i = 0; i < screen.RowLength; i++)
        {
            sprite.LayerMapReserveBlank(TimerMapKey + i);
            timer.LayerStatesToDraw.Add(TimerMapKey + i, null);
            sprite.LayerSetRSI(TimerMapKey + i, new ResPath(TextPath));
            sprite.LayerSetColor(TimerMapKey + i, screen.Color);
            sprite.LayerSetState(TimerMapKey + i, DefaultState);
        }
    }

    /// <summary>
    ///     Called by <see cref="SharedAppearanceSystem.SetData"/> to handle text updates,
    ///     and spawn a <see cref="ScreenTimerComponent"/> if necessary
    /// </summary>
    /// <remarks>
    ///     The appearance updates are batched; order matters for both sender and receiver.
    /// </remarks>
    protected override void OnAppearanceChange(EntityUid uid, ScreenVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!Resolve(uid, ref args.Sprite) || !args.AppearanceData.TryGetValue(ScreenVisuals.Update, out var x) || x is not ScreenUpdate update)
            return;

        component.Updates[update.Priority] = update;

        RecheckUpdates(uid, component, ref args, update);


        // var color = update.Color;
        // var text = update.Text;
        // var timer = update.Timer;

        // if (color != null && color is Color)
        //     component.Color = (Color) color;

        // // DefaultText: fallback text e.g. broadcast updates from comms consoles
        // if (args.AppearanceData.TryGetValue(ScreenVisuals.DefaultText, out var newDefault) && newDefault is string)
        //     component.Text = SegmentText((string) newDefault, component);

        // // ScreenText: currently rendered text e.g. the "ETA" accompanying shuttle timers
        // if (args.AppearanceData.TryGetValue(ScreenVisuals.ScreenText, out var text) && text is string)
        // {
        //     component.TextToDraw = SegmentText((string) text, component);
        //     ClearLayerStates(uid, component);
        //     BuildTextLayers(uid, component, args.Sprite);
        //     DrawLayers(uid, component.LayerStatesToDraw);
        // }

        // if (args.AppearanceData.TryGetValue(ScreenVisuals.TargetTime, out var time) && time is TimeSpan)
        // {
        //     var target = (TimeSpan) time;
        //     if (target > _gameTiming.CurTime)
        //     {
        //         var timer = EnsureComp<ScreenTimerComponent>(uid);
        //         timer.Target = target;
        //         BuildTimerLayers(uid, timer, component);
        //         DrawLayers(uid, timer.LayerStatesToDraw);
        //     }
        //     else
        //     {
        //         OnTimerFinish(uid, component);
        //     }
        // }
    }

    private void RecheckUpdates(EntityUid uid, ScreenVisualsComponent component, ref AppearanceChangeEvent args, ScreenUpdate update)
    {
        if (component.Updates.Count == 0)
            return;

        ScreenUpdate first = component.Updates.First().Value;
        if (first == component.ActiveUpdate)
            return;

        component.ActiveUpdate = first;

        var color = first.Color;
        var text = first.Text;
        var target = first.Timer;

        if (color != null)
        {
            component.Color = color.Value;
        }
        if (text != null)
        {
            component.Text = SegmentText(text, component);
            ClearScreen(uid, component, args.Sprite);
            DrawLayers(uid, component.LayerStatesToDraw);
        }
        if (target != null)
        {
            if (target > _gameTiming.CurTime)
            {
                var timer = EnsureComp<ScreenTimerComponent>(uid);
                timer.Target = target.Value;
                BuildTimerLayers(uid, timer, component);
                DrawLayers(uid, timer.LayerStatesToDraw);
            }
            else
            {
                OnTimerFinish(uid, component);
            }
        }

    }

    /// <summary>
    ///     Removes the timer component, clears the sprite layer dict,
    ///     and draws <see cref="ScreenVisualsComponent.Text"/>
    /// </summary>
    private void OnTimerFinish(EntityUid uid, ScreenVisualsComponent screen)
    {
        if (!TryComp<ScreenTimerComponent>(uid, out var timer) || !TryComp<SpriteComponent>(uid, out var sprite))
            return;

        foreach (var key in timer.LayerStatesToDraw.Keys)
            sprite.RemoveLayer(key);

        RemComp<ScreenTimerComponent>(uid);

        ClearScreen(uid, screen, sprite);
        DrawLayers(uid, screen.LayerStatesToDraw);
    }

    /// <summary>
    ///     Word-wraps (and truncates) string to a string?[] based on
    ///     <see cref="ScreenVisualsComponent.RowLength"/> and <see cref="ScreenVisualsComponent.Rows"/>.
    /// </summary>
    private string?[] SegmentText(string text, ScreenVisualsComponent component)
    {
        int segment = component.RowLength;
        var segmented = new string?[Math.Min(component.Rows, (text.Length - 1) / segment + 1)];

        // populate segmented with a string sliding window using Substring.
        // (Substring(5, 5) will return the 5 characters starting from 5th index)
        // the Mins are for the very short string case, the very long string case, and to not OOB the end of the string.
        for (int i = 0; i < Math.Min(text.Length, segment * component.Rows); i += segment)
            segmented[i / segment] = text.Substring(i, Math.Min(text.Length - i, segment)).Trim();

        return segmented;
    }

    private void ClearScreen(EntityUid uid, ScreenVisualsComponent component, SpriteComponent? sprite = null)
    {
        ClearLayerStates(uid, component, sprite);
        BuildTextLayers(uid, component, sprite);
    }

    /// <summary>
    ///     Clears <see cref="ScreenVisualsComponent.LayerStatesToDraw"/>, and instantiates new blank defaults.
    /// </summary>
    private void ClearLayerStates(EntityUid uid, ScreenVisualsComponent component, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite))
            return;

        foreach (var key in component.LayerStatesToDraw.Keys)
            sprite.RemoveLayer(key);

        component.LayerStatesToDraw.Clear();

        for (var row = 0; row < component.Rows; row++)
            for (var i = 0; i < component.RowLength; i++)
            {
                var key = TextMapKey + row + i;
                sprite.LayerMapReserveBlank(key);
                component.LayerStatesToDraw.Add(key, null);
                sprite.LayerSetRSI(key, new ResPath(TextPath));
                sprite.LayerSetColor(key, component.Color);
                sprite.LayerSetState(key, DefaultState);
            }
    }

    /// <summary>
    ///     Sets the states in the <see cref="ScreenVisualsComponent.LayerStatesToDraw"/> to match the component
    ///     <see cref="ScreenVisualsComponent.Text"/> string?[].
    /// </summary>
    /// <remarks>
    ///     Remember to set <see cref="ScreenVisualsComponent.Text"/> to a string?[] first.
    /// </remarks>
    private void BuildTextLayers(EntityUid uid, ScreenVisualsComponent component, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite))
            return;

        // by rows and then columns
        for (var rowIdx = 0; rowIdx < Math.Min(component.Text.Length, component.Rows); rowIdx++)
        {
            var row = component.Text[rowIdx];
            if (row == null)
                continue;
            var min = Math.Min(row.Length, component.RowLength);

            for (var chr = 0; chr < min; chr++)
            {
                component.LayerStatesToDraw[TextMapKey + rowIdx + chr] = GetStateFromChar(row[chr]);
                sprite.LayerSetOffset(
                    TextMapKey + rowIdx + chr,
                    Vector2.Multiply(
                        new Vector2((chr - min / 2f + 0.5f) * CharWidth, -rowIdx * component.RowOffset),
                        ScreenVisualsComponent.PixelSize
                        ) + component.TextOffset
                );
            }
        }
    }

    /// <summary>
    ///     Populates timer.LayerStatesToDraw & the sprite component's layer dict with calculated offsets.
    /// </summary>
    private void BuildTimerLayers(EntityUid uid, ScreenTimerComponent timer, ScreenVisualsComponent screen)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        string time = TimeToString(
            (_gameTiming.CurTime - timer.Target).Duration(),
            false,
            screen.HourFormat, screen.MinuteFormat, screen.SecondFormat
            );

        int min = Math.Min(time.Length, screen.RowLength);

        for (int i = 0; i < min; i++)
        {
            timer.LayerStatesToDraw[TimerMapKey + i] = GetStateFromChar(time[i]);
            sprite.LayerSetOffset(
                TimerMapKey + i,
                Vector2.Multiply(
                    new Vector2((i - min / 2f + 0.5f) * CharWidth, 0f),
                    ScreenVisualsComponent.PixelSize
                    ) + screen.TimerOffset
            );
        }
    }

    /// <summary>
    ///     Draws a LayerStates dict by setting the sprite states individually.
    /// </summary>
    private void DrawLayers(EntityUid uid, Dictionary<string, string?> layerStates, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite))
            return;

        foreach (var (key, state) in layerStates.Where(pairs => pairs.Value != null))
            sprite.LayerSetState(key, state);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ScreenTimerComponent, ScreenVisualsComponent>();
        while (query.MoveNext(out var uid, out var timer, out var screen))
        {
            if (timer.Target < _gameTiming.CurTime)
            {
                OnTimerFinish(uid, screen);
                continue;
            }

            BuildTimerLayers(uid, timer, screen);
            DrawLayers(uid, timer.LayerStatesToDraw);
        }
    }

    /// <summary>
    ///     Returns the <paramref name="timeSpan"/> converted to a string in either HH:MM, MM:SS or potentially SS:mm format.
    /// </summary>
    /// <param name="timeSpan">TimeSpan to convert into string.</param>
    /// <param name="getMilliseconds">Should the string be ss:ms if minutes are less than 1?</param>
    /// <remarks>
    ///     hours, minutes, seconds, and centiseconds are each set to 2 decimal places by default.
    /// </remarks>
    public static string TimeToString(TimeSpan timeSpan, bool getMilliseconds = true, string hours = "D2", string minutes = "D2", string seconds = "D2", string cs = "D2")
    {
        string firstString;
        string lastString;

        if (timeSpan.TotalHours >= 1)
        {
            firstString = timeSpan.Hours.ToString(hours);
            lastString = timeSpan.Minutes.ToString(minutes);
        }
        else if (timeSpan.TotalMinutes >= 1 || !getMilliseconds)
        {
            firstString = timeSpan.Minutes.ToString(minutes);
            lastString = timeSpan.Seconds.ToString(seconds);
        }
        else
        {
            firstString = timeSpan.Seconds.ToString(seconds);
            var centiseconds = timeSpan.Milliseconds / 10;
            lastString = centiseconds.ToString(cs);
        }

        return firstString + ':' + lastString;
    }

    /// <summary>
    ///     Returns the Effects/text.rsi state string based on <paramref name="character"/>, or null if none available.
    /// </summary>
    public static string? GetStateFromChar(char? character)
    {
        if (character == null)
            return null;

        // First checks if its one of our special characters
        if (CharStatePairs.ContainsKey(character.Value))
            return CharStatePairs[character.Value];

        // Or else it checks if its a normal letter or digit
        if (char.IsLetterOrDigit(character.Value))
            return character.Value.ToString().ToLower();

        return null;
    }
}
