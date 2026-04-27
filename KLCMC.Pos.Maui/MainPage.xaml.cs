using KLCMC.Pos.Core.ViewModels;

namespace KLCMC.Pos.Maui;

public partial class MainPage : ContentPage
{
    private enum MainTab
    {
        CurrentSale,
        DailyAccount,
        Configure
    }

    public DailyAccountViewModel DailyAccountViewModel { get; }

    public MainPage(MainViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        DailyAccountViewModel = services.GetRequiredService<DailyAccountViewModel>();
        SetActiveTab(MainTab.CurrentSale);
    }

    private void OnCurrentSaleTabClicked(object? sender, EventArgs e)
    {
        SetActiveTab(MainTab.CurrentSale);
    }

    private void OnDailyAccountTabClicked(object? sender, EventArgs e)
    {
        SetActiveTab(MainTab.DailyAccount);
        DailyAccountViewModel.Load();
    }

    private void OnConfigureTabClicked(object? sender, EventArgs e)
    {
        SetActiveTab(MainTab.Configure);
    }

    private void SetActiveTab(MainTab tab)
    {
        var isCurrentSale = tab == MainTab.CurrentSale;
        var isDailyAccount = tab == MainTab.DailyAccount;
        var isConfigure = tab == MainTab.Configure;

        CurrentSaleTabContent.IsVisible = isCurrentSale;
        DailyAccountTabContent.IsVisible = isDailyAccount;
        ConfigureTabContent.IsVisible = isConfigure;

        CurrentSaleTabButton.BackgroundColor = isCurrentSale ? Color.FromArgb("#1F3D5C") : Color.FromArgb("#253648");
        CurrentSaleTabButton.TextColor = isCurrentSale ? Color.FromArgb("#D2E4FB") : Color.FromArgb("#8EA0B9");

        DailyAccountTabButton.BackgroundColor = isDailyAccount ? Color.FromArgb("#1F3D5C") : Color.FromArgb("#253648");
        DailyAccountTabButton.TextColor = isDailyAccount ? Color.FromArgb("#D2E4FB") : Color.FromArgb("#8EA0B9");

        ConfigureTabButton.BackgroundColor = isConfigure ? Color.FromArgb("#1F3D5C") : Color.FromArgb("#253648");
        ConfigureTabButton.TextColor = isConfigure ? Color.FromArgb("#D2E4FB") : Color.FromArgb("#8EA0B9");
    }
}
