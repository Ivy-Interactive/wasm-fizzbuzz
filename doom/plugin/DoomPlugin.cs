using Ivy.Plugins;
using Ivy.Tendril.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

[assembly: IvyPlugin(typeof(Ivy.Tendril.Plugin.Doom.DoomPlugin))]

namespace Ivy.Tendril.Plugin.Doom;

public class DoomPlugin : IIvyPlugin<ITendrilExtendedPluginContext>
{
    internal static string WadsDirectory { get; private set; } = "";
    internal static Action? OpenAnnoyingDialog { get; private set; }
    internal static bool AnnoyingPopupsEnabled { get; private set; }

    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Tendril.Plugin.Doom",
        Title = "DOOM",
        Version = new Version(1, 0, 0),
        Icon = PluginIcon.Named("Skull"),
    };

    public PluginConfigurationSchema ConfigurationSchema { get; } = new SchemaBuilder()
        .AddBoolean("AnnoyingPopups", defaultValue: false, description: "Enable annoying popups that interrupt gameplay at the worst moments")
        .Build();

    public void Configure(ITendrilExtendedPluginContext context)
    {
        WadsDirectory = Path.Combine(context.TendrilHome, "doom-wads");
        Directory.CreateDirectory(WadsDirectory);

        AnnoyingPopupsEnabled = context.Config.GetValue("AnnoyingPopups") == "true";

        OpenAnnoyingDialog = context.RegisterDialog(
            "$doom-annoying",
            dialogOpen => new DoomAnnoyingDialog(dialogOpen));

        context.AddApp(new AppDescriptor
        {
            Id = "doom",
            Title = "DOOM",
            Icon = Icons.Skull,
            IsVisible = true,
            Group = ["Apps"],
            Order = 666,
            ViewFactory = () => new DoomView(),
        });

        context.UseWebApplication(app =>
        {
            // Serve doom.wasm from embedded resources
            app.MapGet("/ivy/plugins/doom/doom.wasm", () =>
            {
                var assembly = typeof(DoomPlugin).Assembly;
                var stream = assembly.GetManifestResourceStream("Ivy.Tendril.Plugin.Doom.frontend.public.doom.wasm");
                if (stream == null)
                    return Results.NotFound();
                return Results.File(stream, "application/wasm");
            });

            // List available WADs
            app.MapGet("/ivy/plugins/doom/wads", () =>
            {
                if (!Directory.Exists(WadsDirectory))
                    return Results.Ok(Array.Empty<string>());

                var wads = Directory.GetFiles(WadsDirectory, "*.wad")
                    .Select(Path.GetFileName)
                    .OrderBy(n => n)
                    .ToArray();
                return Results.Ok(wads);
            });

            // Serve a WAD file
            app.MapGet("/ivy/plugins/doom/wads/{name}", (string name) =>
            {
                if (name.Contains("..") || name.Contains('/') || name.Contains('\\'))
                    return Results.BadRequest();

                var path = Path.Combine(WadsDirectory, name);
                if (!File.Exists(path))
                    return Results.NotFound();

                return Results.File(path, "application/octet-stream");
            });
        });
    }
}
