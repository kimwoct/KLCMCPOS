using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Maui;

public sealed class MauiConfirmDialog : IConfirmDialog
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
        => Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
}
