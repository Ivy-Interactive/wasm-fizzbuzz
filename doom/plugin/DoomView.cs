using Ivy;

namespace Ivy.Tendril.Plugin.Doom;

public class DoomView : ViewBase
{
    public override object Build()
    {
        var selectedWad = UseState<string?>(GetDefaultWad);
        var uploadedFile = UseState<FileUpload<byte[]>?>(null);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(uploadedFile));
        var resetCounter = UseState(0);
        var paused = UseState(false);

        // Save uploaded WAD to disk
        if (uploadedFile.Value is { Content: not null })
        {
            var fileName = uploadedFile.Value.FileName;
            var destPath = Path.Combine(DoomPlugin.WadsDirectory, fileName);
            File.WriteAllBytes(destPath, uploadedFile.Value.Content);
            uploadedFile.Set(null);
            selectedWad.Set(fileName);
        }

        var availableWads = GetAvailableWads();
        var wadUrl = selectedWad.Value != null
            ? $"/ivy/plugins/doom/wads/{selectedWad.Value}?r={resetCounter.Value}"
            : null;

        return Layout.Vertical().Padding(4).Gap(4)
            | (Layout.Horizontal().Gap(2)
                | selectedWad.ToSelectInput(
                    availableWads.Select(w => new Option<string?>(w, w)).ToArray(),
                    placeholder: "Select WAD...")
                | new Button(paused.Value ? "Play" : "Pause", onClick: _ =>
                {
                    paused.Set(!paused.Value);
                    return ValueTask.CompletedTask;
                }, icon: paused.Value ? Icons.Play : Icons.Pause, variant: ButtonVariant.Outline)
                | new Button("Restart", onClick: _ =>
                {
                    paused.Set(false);
                    resetCounter.Set(resetCounter.Value + 1);
                    return ValueTask.CompletedTask;
                }, icon: Icons.RotateCcw, variant: ButtonVariant.Outline)
                | new Button("Delete WAD", onClick: _ =>
                {
                    if (selectedWad.Value == null) return ValueTask.CompletedTask;
                    var path = Path.Combine(DoomPlugin.WadsDirectory, selectedWad.Value);
                    if (File.Exists(path)) File.Delete(path);
                    var remaining = GetAvailableWads();
                    selectedWad.Set(remaining.Length > 0 ? remaining[0] : null);
                    return ValueTask.CompletedTask;
                }, icon: Icons.Trash2, variant: ButtonVariant.Outline) { Disabled = selectedWad.Value == null }
                | uploadedFile.ToFileInput(upload)
                    .Accept(".wad")
                    .Placeholder("Upload WAD")
                    .Variant(FileInputVariant.Default))
            | (wadUrl != null
                ? (object)new DoomWidget { CanvasWidth = 640, CanvasHeight = 400, WadUrl = wadUrl, Paused = paused.Value }
                : Callout.Warning("No WAD file selected. Upload a .wad file or select one from the dropdown."));
    }

    private static string? GetDefaultWad()
    {
        var wads = GetAvailableWads();
        return wads.Length > 0 ? wads[0] : null;
    }

    private static string[] GetAvailableWads()
    {
        if (!Directory.Exists(DoomPlugin.WadsDirectory))
            return [];
        return Directory.GetFiles(DoomPlugin.WadsDirectory, "*.wad")
            .Select(Path.GetFileName)
            .Where(n => n != null)
            .Cast<string>()
            .OrderBy(n => n)
            .ToArray();
    }
}
