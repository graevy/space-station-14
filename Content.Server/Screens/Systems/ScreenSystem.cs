using Content.Shared.Screen;
using Content.Server.Screen.Components;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork;
using Robust.Shared.Timing;


namespace Content.Server.Screen.Systems;

/// <summary>
/// Controls the wallmounted screens on stations and shuttles displaying e.g. FTL duration, ETA
/// </summary>
public sealed class ScreenSystem : EntitySystem
{
    // [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    // [Dependency] private readonly DeviceNetworkSystem _network = default!;

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
    }
}
