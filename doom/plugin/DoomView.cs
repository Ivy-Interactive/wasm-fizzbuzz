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

        var shotsFired = UseState(0);
        var lastAnnoyedAt = UseState(0);
        var lastMissAnnoyTime = UseState<DateTime>(DateTime.MinValue);

        var availableWads = GetAvailableWads();
        var wadUrl = selectedWad.Value != null
            ? $"/ivy/plugins/doom/wads/{selectedWad.Value}?r={resetCounter.Value}"
            : null;

        var annoy = DoomPlugin.AnnoyingPopupsEnabled;

        return Layout.Vertical().Padding(4).Gap(3)
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
                ? (object)new DoomWidget
                {
                    CanvasWidth = 640, CanvasHeight = 400, WadUrl = wadUrl, Paused = paused.Value,
                    OnStateChanged = !annoy ? null : new(e =>
                    {
                        var health = e.Value.Health;
                        if (health is > 0 and <= 50 && shotsFired.Value - lastAnnoyedAt.Value > 20)
                        {
                            lastAnnoyedAt.Set(shotsFired.Value);
                            DoomPlugin.OpenAnnoyingDialog?.Invoke();
                        }
                        return ValueTask.CompletedTask;
                    }),
                    OnWeaponFired = !annoy ? null : new(e =>
                    {
                        var count = shotsFired.Value + 1;
                        shotsFired.Set(count);
                        if (count % 50 == 0 && count - lastAnnoyedAt.Value >= 30)
                        {
                            lastAnnoyedAt.Set(count);
                            DoomPlugin.OpenAnnoyingDialog?.Invoke();
                        }
                        return ValueTask.CompletedTask;
                    }),
                    OnShotLanded = !annoy ? null : new(e =>
                    {
                        if (e.Value.Hit == null && DateTime.UtcNow - lastMissAnnoyTime.Value > TimeSpan.FromMilliseconds(500))
                        {
                            lastMissAnnoyTime.Set(DateTime.UtcNow);
                            DoomPlugin.OpenAnnoyingDialog?.Invoke();
                        }
                        return ValueTask.CompletedTask;
                    }),
                }
                : Callout.Warning("No WAD file selected. Upload a .wad file or select one from the dropdown."))
            | Text.Muted("Arrow keys to move, Ctrl to shoot, Space to open doors, Enter to start/menu.")
            | new Expandable(
                Text.Muted("Third-Party Licenses"),
                Layout.Vertical().Gap(2)
                    | Text.Label("DOOM Engine (doom.wasm)")
                    | Text.Muted("Source: github.com/Ivy-Interactive/wasm-fizzbuzz (forked from diekmann/wasm-fizzbuzz) — DOOM Source Code License (educational/non-commercial). Original wasm port by diekmann, CC BY-NC.")
                    | Text.Label("musl libc")
                    | Text.Muted("MIT License — musl.libc.org")
                    | Text.Label("clang compiler-rt")
                    | Text.Muted("Apache 2.0 with LLVM Exception — github.com/llvm/llvm-project")
                    | Text.Label("lazy_static crate")
                    | Text.Muted("MIT OR Apache-2.0 (dual-licensed)")
            );
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
