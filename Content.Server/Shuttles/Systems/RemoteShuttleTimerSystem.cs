using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;

namespace Content.Server.Shuttles.Systems
{
    /// <summary>
    /// shuttle timers that are not shuttle children, but behave similarly, e.g. hallway evac timers
    /// </summary>
    public sealed partial class ShuttleTimerSystem : EntitySystem
    {
        [Dependency] private readonly ArrivalsSystem _arrivalsSystem = default!;

        /// <summary>
        /// Called by a remote to pair with shuttles
        /// </summary>
        private void PairRemoteWithShuttles(EntityUid uid, RemoteShuttleTimerComponent component, ComponentStartup args)
        {
            var query = EntityQuery<ShuttleTimerComponent, ShuttleComponent>(true);
            foreach (var (shuttleTimerComp, _) in query)
            {
                if (shuttleTimerComp.PairWith == component.PairWith)
                    shuttleTimerComp.RemoteScreens.Add(uid);
            }
        }

        /// <summary>
        /// Called by a shuttle to pair with remotes
        /// </summary>
        public void PairShuttleWithRemotes(ShuttleTimerComponent shuttleTimerComp, RemoteShuttleTimerMask pairingComp)
        {
            var query = AllEntityQuery<RemoteShuttleTimerComponent>();
            while (query.MoveNext(out var timerUid, out var timerComp))
            {
                if (timerComp.PairWith == pairingComp)
                    shuttleTimerComp.RemoteScreens.Add(timerUid);
            }
        }

        // hesitant to simplify this code because the remote timer filtering delegate might change later?
        // right now, each ftl event handler could pass a "roundtrip" timespan, eliminating this bloat.
        // each filtering delegate could also be reduced to a bool
        /// <summary>Handles arrivals-specific dock timings.</summary>
        public void OnArrivalsDocked(EntityUid uid, ArrivalsShuttleComponent shuttleComp, ref FTLCompletedEvent args)
        {
            if (!HasComp<ShuttleTimerComponent>(uid))
                return;

            var local = new LocalShuttleTimerEvent(TimeSpan.FromSeconds(_arrivalsSystem.Docked));
            RaiseLocalEvent(uid, ref local);

            // remote timers at the origin (where we just docked)
            var originRemote = new RemoteShuttleTimerEvent
            (
                TimeSpan.FromSeconds(_arrivalsSystem.Docked),
                new MapFilter
                {
                    Comparator = (timerMapUid, shuttleMapUid) => timerMapUid == shuttleMapUid,
                    MapUid = args.MapUid
                }
            );
            RaiseLocalEvent(uid, ref originRemote);
        }

        public void OnArrivalsFTL(EntityUid uid, ArrivalsShuttleComponent component, ref FTLStartedEvent args)
        {
            if (!TryComp<FTLComponent>(uid, out var ftlComp) || args.FromMapUid == null)
                return;

            var originRemote = new RemoteShuttleTimerEvent
            (
                TimeSpan.FromSeconds(_arrivalsSystem.Cooldown + _arrivalsSystem.Travel),
                new MapFilter
                {
                    Comparator = (timerMapUid, shuttleMapUid) => timerMapUid == shuttleMapUid,
                    MapUid = (EntityUid) args.FromMapUid
                }
            );
            RaiseLocalEvent(uid, ref originRemote);
        }
    }
}
