namespace Content.Server.Shuttles.Components
{
    /// <summary>
    /// Shuttle timers that aren't on their shuttle e.g. the hallway evac timers
    /// </summary>
    [RegisterComponent]
    public sealed partial class RemoteShuttleTimerComponent : Component
    {
        /// <summary>
        /// Bitmask to sync with shuttlecomponents e.g. <see cref="EmergencyShuttleComponent"/>
        /// </summary>
        [DataField("pairWith"), ViewVariables]
        public RemoteShuttleTimerMask PairWith;
    }

    [Flags]
    public enum RemoteShuttleTimerMask : byte
    {
        None = 0,
        Emergency = 1 << 0,
        Arrivals = 1 << 1,
        Cargo = 1 << 2,
        Salvage = 1 << 3,
    }

}
