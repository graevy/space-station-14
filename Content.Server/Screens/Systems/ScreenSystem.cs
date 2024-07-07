using Content.Shared.Screen;
using Content.Server.Screens.Components;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork;
using Robust.Shared.Timing;


namespace Content.Server.Screens.Systems;

/// <summary>
/// Controls the wallmounted screens on stations and shuttles displaying e.g. FTL duration, ETA
/// </summary>
public sealed class ScreenSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly DeviceNetworkSystem _network = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreenComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
    }

    /// <summary>
    /// Determines if/how a packet affects this screen.
    /// Currently there are 2 broadcast domains: Arrivals, and every other screen.
    /// Domain is determined by the <see cref="DeviceNetworkComponent.TransmitFrequency"/> on each timer.
    /// Each broadcast domain is divided into subnets. Screen MapUid determines subnet.
    /// So far I haven't needed more than per-map update granularity
    /// </summary>
    private void OnPacketReceived(EntityUid uid, ScreenComponent component, DeviceNetworkPacketEvent args)
    {
        args.Data.TryGetValue(ScreenMasks.Updates, out ScreenUpdate[]? updates);
        if (updates == null)
            return;

        // drop the packet if it's intended for a subnet (MapUid) that doesn't match our screen's
        var timerXform = Transform(uid);
        if (timerXform.MapUid == null)
            return;

        foreach (var update in updates)
            // the griduid check handled some null mapuid edge case involving hyperspace iirc
            if (update != null && update.Subnet == timerXform.MapUid || update.Subnet == timerXform.GridUid)
                _appearanceSystem.SetData(uid, ScreenVisuals.Update, update);


        // if (update.Text != null)
        //     ScreenText(uid, component, update);
        // if (update.Timer != null)
        //     ScreenTimer(uid, component, update);
        // if (update.Color != null)
        //     _appearanceSystem.SetData(uid, ScreenVisuals.Color, update.Color.Value);
    }

    // public void SendUpdate(EntityUid uid, ScreenUpdate update, uint frequency)
    // {
    //     var payload = new NetworkPayload { [ScreenMasks.Update] = update };
    //     _network.QueuePacket(uid, null, payload, frequency);
    // }

    /// <summary>
    ///     Send a text update to a specific screen.
    /// </summary>
    // private void ScreenText(EntityUid uid, ScreenComponent component, ScreenUpdate update)
    // {
    //     // don't allow text updates if there's an active timer
    //     // (and just check here so the server doesn't have to track them)
    //     // if (_appearanceSystem.TryGetData(uid, ScreenVisuals.TargetTime, out TimeSpan target)
    //     //     && target > _gameTiming.CurTime)
    //     //     return;

    //     var text = update.Text;
    //     if (text != null)
    //     {
    //         _appearanceSystem.SetData(uid, ScreenVisuals.DefaultText, text);
    //         _appearanceSystem.SetData(uid, ScreenVisuals.ScreenText, text);
    //     }
    // }

    // private void ScreenTimer(EntityUid uid, ScreenComponent component, ScreenUpdate update)
    // {
    //     var timer = update.Timer;
    //     if (timer != null)
    //         _appearanceSystem.SetData(uid, ScreenVisuals.TargetTime, _gameTiming.CurTime + timer.Value);
    // }
}
