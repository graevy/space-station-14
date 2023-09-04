using Content.Server.DeviceLinking;
using Content.Server.TextScreen.Components;
using Content.Server.TextScreen.Events;

using Content.Shared.TextScreen;

using Robust.Shared.Timing;


namespace Content.Server.TextScreen;

public sealed class TextScreenSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TextScreenComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<TextScreenComponent, TextScreenTimerEvent>(OnTimer);
        SubscribeLocalEvent<TextScreenComponent, TextScreenTextEvent>(OnText);
    }

    private void OnInit(EntityUid uid, TextScreenComponent component, ComponentInit args)
    {
        _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, component.Label);
    }

    private void OnTimer(EntityUid uid, TextScreenComponent component, ref TextScreenTimerEvent args)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        if (appearance != null)
        {
            component.Remaining = _gameTiming.CurTime + args.Duration;
            _appearanceSystem.SetData(uid, TextScreenVisuals.Mode, TextScreenMode.Timer, appearance);
            _appearanceSystem.SetData(uid, TextScreenVisuals.TargetTime, component.Remaining, appearance);
        }
    }

    private void OnText(EntityUid uid, TextScreenComponent component, ref TextScreenTextEvent args)
    {
        component.Remaining = null;
        component.Label = args.Label[..Math.Min(5, args.Label.Length)];

        _appearanceSystem.SetData(uid, TextScreenVisuals.Mode, TextScreenMode.Text);
        _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, component.Label);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<TextScreenComponent>();
        while (query.MoveNext(out var uid, out var timer))
        {
            if (timer.Remaining != null && timer.Remaining > _gameTiming.CurTime)
                continue;

            Finish(uid, timer);

            if (timer.DoneSound != null)
                _audio.PlayPvs(timer.DoneSound, uid);
        }
    }

    private void Finish(EntityUid uid, TextScreenComponent component)
    {
        component.Remaining = null;
        _appearanceSystem.SetData(uid, TextScreenVisuals.Mode, TextScreenMode.Text);
    }
}
