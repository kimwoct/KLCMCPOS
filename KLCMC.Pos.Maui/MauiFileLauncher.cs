using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Maui;

/// <summary>Opens a local file via MAUI Launcher — works on macOS Catalyst and Windows.</summary>
public sealed class MauiFileLauncher : IFileLauncher
{
    public async Task OpenFileAsync(string filePath)
    {
        var uri = new Uri($"file://{filePath}");
        await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(uri);
    }
}
