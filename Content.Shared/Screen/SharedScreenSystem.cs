

namespace Content.Shared.Screen.Systems;

public abstract class SharedScreenSystem : EntitySystem
{
    // [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected void SendUpdate(EntityUid uid, ScreenUpdate update)
    {

    }


}