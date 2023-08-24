using System.Collections.Generic;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.TextScreen.Components;
using Content.Server.TextScreen.Events;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Server.DeviceLinking.Components;
using Content.Shared.MachineLinking;
using Content.Shared.TextScreen;
using Content.Shared.CCVar;
using Robust.Shared.Map;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;


// TODO:
// - emergency shuttle recall inverts timer?
//    i saw this happen once. had to do with removing the activecomponent early
// - deduplicate signaltimer with a maintainer's blessing
// - scan UI?
// - could filter with radio bands instead of mapIDs
// - redundant pairing is messy. different pattern would be nice
//    bitmasks -> compile-time component typing would make a lot of linear code constant.
//      could directly access traitdicts at runtime?

namespace Content.Server.Shuttles.Systems
{
    public struct MapFilter
    {
        public Func<EntityUid, EntityUid, bool> Comparator;
        /// <summary>the MapUid that each timer's map is filtered against</summary>
        public EntityUid MapUid;
    }

    public sealed partial class ShuttleTimerSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShuttleTimerComponent, ComponentStartup>(OnComponentStartup);
            SubscribeLocalEvent<RemoteShuttleTimerComponent, ComponentStartup>(PairRemoteWithShuttles);

            SubscribeLocalEvent<ShuttleTimerComponent, AllShuttleTimerEvent>(OnAll);
            SubscribeLocalEvent<ShuttleTimerComponent, LocalShuttleTimerEvent>(OnLocal);
            SubscribeLocalEvent<ShuttleTimerComponent, RemoteShuttleTimerEvent>(OnRemote);
            SubscribeLocalEvent<ShuttleTimerComponent, FTLStartedEvent>(OnFTLStart);
        }
        /// <summary>
        /// Attach child shuttletimer screens to parent shuttletimer components.
        /// </summary>
        private void OnComponentStartup(EntityUid uid, ShuttleTimerComponent timerComp, ComponentStartup args)
        {
            if (!TryComp<TransformComponent>(uid, out var xform))
                return;

            if (!TryComp<ShuttleTimerComponent>(xform.ParentUid, out var parentComp) ||
                !HasComp<ShuttleComponent>(xform.ParentUid))
                return;

            parentComp.LocalScreens.Add(uid);
        }

        private void OnFTLStart(EntityUid uid, ShuttleTimerComponent component, ref FTLStartedEvent args)
        {
            if (!TryComp<FTLComponent>(uid, out var ftl))
                return;

            UpdateLocalScreens(component, TimeSpan.FromSeconds(ftl.TravelTime));
        }

        private void OnAll(EntityUid uid, ShuttleTimerComponent component, ref AllShuttleTimerEvent args)
        {
            UpdateLocalScreens(component, args.Countdown);
            UpdateRemoteScreens(component, args.Countdown);
        }

        private void OnLocal(EntityUid uid, ShuttleTimerComponent component, ref LocalShuttleTimerEvent args)
        {
            UpdateLocalScreens(component, args.Countdown);
        }

        private void OnRemote(EntityUid uid, ShuttleTimerComponent component, ref RemoteShuttleTimerEvent args)
        {
            if (args.Filter == null)
                UpdateRemoteScreens(component, args.Countdown);
            else
                UpdateRemoteScreens(component, args.Countdown, (MapFilter) args.Filter);
        }

        /// <summary>
        /// Update all child screens of parent <see cref="ShuttleTimerComponent"/>.
        /// </summary>
        private void UpdateLocalScreens(ShuttleTimerComponent component, TimeSpan duration)
        {
            foreach (var timerUid in component.LocalScreens)
            {
                RemComp<ActiveTextScreenTimerComponent>(timerUid);

                // this kept not triggering on TimeSpan.Zero
                if (duration <= TimeSpan.MinValue)
                    continue;

                // set delay and start
                var ev = new TextScreenTimerEvent(duration);
                RaiseLocalEvent(timerUid, ref ev);
            }
        }

        /// <summary>
        /// Update all synced remote screens in a <see cref="ShuttleTimerComponent"/>. Filter by delegate.
        /// </summary>
        /// <param name="filter">Delegate determining remote timer update eligibility from shuttleUid's map</param>
        private void UpdateRemoteScreens(ShuttleTimerComponent component, TimeSpan duration, MapFilter filter)
        {
            foreach (var timerUid in component.RemoteScreens)
            {
                if (!TryComp<TransformComponent>(timerUid, out var timerXform) || timerXform.MapUid == null)
                    continue;

                if (!filter.Comparator(timerXform.MapUid.Value, filter.MapUid))
                    continue;

                RemComp<ActiveTextScreenTimerComponent>(timerUid);

                if (duration <= TimeSpan.MinValue)
                    continue;

                var ev = new TextScreenTimerEvent(duration);
                RaiseLocalEvent(timerUid, ref ev);
            }
        }

        /// <summary>
        /// Update all synced remote screens in a <see cref="ShuttleTimerComponent"/>. No filtering.
        /// </summary>
        private void UpdateRemoteScreens(ShuttleTimerComponent component, TimeSpan duration)
        {
            foreach (var timerUid in component.RemoteScreens)
            {
                RemComp<ActiveTextScreenTimerComponent>(timerUid);

                if (duration <= TimeSpan.MinValue)
                    continue;

                var ev = new TextScreenTimerEvent(duration);
                RaiseLocalEvent(timerUid, ref ev);
            }
        }

        // never got an answer figuring out how to do this. there's definitely a better pattern
        /// <summary>
        /// Raises a TEvent on each entity containing TComp.
        /// </summary>
        public void RaiseEventOnShuttles<TComp, TEvent>(ref TEvent ev)
            where TComp : Component
            where TEvent : struct
        {
            var query = AllEntityQuery<TComp>();
            while (query.MoveNext(out var uid, out _))
            {
                RaiseLocalEvent(uid, ref ev);
            }
        }

        // backend of a feature to pair a shuttle timer manually, for player-created shuttles
        /// <summary>
        /// Sort all <see cref="ShuttleComponent"/>s in radar range by proximity.
        /// </summary>
        // private void Scan(EntityUid uid)
        // {
        //     var allShuttles = new SortedList<float, EntityUid>();
        //     if (!TryComp<TransformComponent>(uid, out var screenXform))
        //         return;

        //     TryComp<RadarConsoleComponent>(uid, out var radar);
        //     var range = radar?.MaxRange ?? SharedRadarConsoleSystem.DefaultMaxRange;

        //     // quadratic w/ number of shuttles
        //     var shuttleQuery = AllEntityQuery<ShuttleComponent, ShuttleTimerComponent, TransformComponent>();
        //     while (shuttleQuery.MoveNext(out var shuttleUid, out var shuttleComp, out var shuttleTimerComp, out var shuttleXform))
        //     {
        //         if (shuttleXform.GridUid != screenXform.GridUid || shuttleUid == uid)
        //             continue;

        //         var distance = (screenXform.MapPosition.Position - shuttleXform.MapPosition.Position).Length();
        //         if (distance >= range)
        //             continue;

        //         allShuttles.Add(distance, shuttleUid);
        //     }
        // }
    }
}
