namespace Ivy.Tendril.Plugin.Doom;

[ExternalWidget("frontend/dist/Ivy_Tendril_Plugin_Doom.js", ExportName = "Doom")]
public record DoomWidget : WidgetBase<DoomWidget>
{
    [Prop] public int CanvasWidth { get; init; } = 640;
    [Prop] public int CanvasHeight { get; init; } = 400;
    [Prop] public string? WadUrl { get; init; }
    [Prop] public bool Paused { get; init; }

    [Event] public EventHandler<Event<DoomWidget, DoomGameState>>? OnStateChanged { get; init; }
    [Event] public EventHandler<Event<DoomWidget, DoomWeaponEvent>>? OnWeaponFired { get; init; }
    [Event] public EventHandler<Event<DoomWidget, DoomShotLandedEvent>>? OnShotLanded { get; init; }
    [Event] public EventHandler<Event<DoomWidget, DoomEnemyKilledEvent>>? OnEnemyKilled { get; init; }
}

public record DoomGameState
{
    public int Health { get; init; }
    public int Armor { get; init; }
    public int ArmorType { get; init; }
    public string Weapon { get; init; } = "";
    public DoomAmmo Ammo { get; init; } = new();
}

public record DoomAmmo
{
    public int Bullets { get; init; }
    public int Shells { get; init; }
    public int Cells { get; init; }
    public int Rockets { get; init; }
}

public record DoomWeaponEvent
{
    public string Weapon { get; init; } = "";
}

public record DoomShotLandedEvent
{
    public string Weapon { get; init; } = "";
    public string? Hit { get; init; }
}

public record DoomEnemyKilledEvent
{
    public string Enemy { get; init; } = "";
    public string Weapon { get; init; } = "";
}
