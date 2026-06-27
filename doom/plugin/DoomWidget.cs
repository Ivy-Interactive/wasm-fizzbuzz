namespace Ivy.Tendril.Plugin.Doom;

[ExternalWidget("frontend/dist/Ivy_Tendril_Plugin_Doom.js", ExportName = "Doom")]
public record DoomWidget : WidgetBase<DoomWidget>
{
    [Prop] public int CanvasWidth { get; init; } = 640;
    [Prop] public int CanvasHeight { get; init; } = 400;
    [Prop] public string? WadUrl { get; init; }
    [Prop] public bool Paused { get; init; }
}
