using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Maui;

/// <summary>Shares/exports a local file using the platform share sheet (macOS Catalyst + Windows).</summary>
public sealed class MauiFileLauncher : IFileLauncher
{
    public async Task OpenFileAsync(string filePath)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = System.IO.Path.GetFileName(filePath),
            File = new ShareFile(filePath)
        });
    }
}
