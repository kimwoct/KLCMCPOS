namespace KLCMC.Pos.Core.Services;

/// <summary>Opens a local file with the platform's default application.</summary>
public interface IFileLauncher
{
    Task OpenFileAsync(string filePath);
}
