using Ivy;

namespace Ivy.Tendril.Plugin.Doom;

public class DoomView : ViewBase
{
    public override object Build()
    {
        return Layout.Vertical().Padding(4).Gap(4)
            | Text.H1("DOOM")
            | Text.Muted("Click the canvas to capture keyboard input. Arrow keys to move, Ctrl to shoot, Space to open doors, Enter to start.")
            | new DoomWidget { CanvasWidth = 640, CanvasHeight = 400 };
    }
}
