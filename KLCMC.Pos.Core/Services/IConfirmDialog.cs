namespace KLCMC.Pos.Core.Services;

/// <summary>Shows a platform confirmation dialog and returns true if the user confirmed.</summary>
public interface IConfirmDialog
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
