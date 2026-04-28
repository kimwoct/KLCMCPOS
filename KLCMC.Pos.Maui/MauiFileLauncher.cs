using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Maui;

/// <summary>Saves exported files to ~/Downloads and opens them with the default app.</summary>
public sealed class MauiFileLauncher : IFileLauncher
{
    public async Task OpenFileAsync(string filePath)
    {
        // Open with the default application via MAUI Launcher
        await Launcher.OpenAsync(new OpenFileRequest
        {
            File = new ReadOnlyFile(filePath)
        });
    }
}
