using KLCMC.Pos.Core.ViewModels;

namespace KLCMC.Pos.Maui;

public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _services;

    public MainPage(MainViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;
    }

    private async void OnDailyAccountClicked(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<DailyAccountPage>();
        if (page.BindingContext is DailyAccountViewModel vm)
        {
            vm.Load();
        }
        await Navigation.PushAsync(page);
    }
}
