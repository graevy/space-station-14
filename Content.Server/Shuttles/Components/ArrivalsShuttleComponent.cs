using Content.Server.Shuttles.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Shuttles.Components;

[RegisterComponent, Access(typeof(ArrivalsSystem))]
public sealed partial class ArrivalsShuttleComponent : Component
{
    [DataField("station")]
    public EntityUid Station;

    [DataField("nextTransfer", customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextTransfer;

    [DataField("nextArrivalsTime", customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextArrivalsTime;

    [DataField("cooldown")]
    public float Cooldown;

    [DataField("startup")]
    public float Startup;

    [DataField("travel")]
    public float Travel;

    [DataField("arrival")]
    public float Arrival;

    [DataField("dockTime")]
    public float DockTime;
}
