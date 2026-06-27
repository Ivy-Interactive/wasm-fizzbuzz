using Ivy.Plugins;
using Ivy.Tendril.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

[assembly: IvyPlugin(typeof(Ivy.Tendril.Plugin.Doom.DoomPlugin))]

namespace Ivy.Tendril.Plugin.Doom;

public class DoomPlugin : IIvyPlugin<ITendrilExtendedPluginContext>
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Tendril.Plugin.Doom",
        Title = "DOOM",
        Version = new Version(1, 0, 0),
        Icon = PluginIcon.Named("Skull"),
    };

    public PluginConfigurationSchema? ConfigurationSchema => null;

    public void Configure(ITendrilExtendedPluginContext context)
    {
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
            app.MapGet("/ivy/plugins/doom/{fileName}", (string fileName) =>
            {
                var assembly = typeof(DoomPlugin).Assembly;
                var resourceName = $"Ivy.Tendril.Plugin.Doom.frontend.public.{fileName}";

                var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                    return Results.NotFound();

                var contentType = fileName switch
                {
                    "doom.wasm" => "application/wasm",
                    "doom1.wad" => "application/octet-stream",
                    _ => "application/octet-stream"
                };

                return Results.File(stream, contentType);
            });
        });
    }
}
