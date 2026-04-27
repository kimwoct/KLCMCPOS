using KLCMC.Pos.Core.ViewModels;

namespace KLCMC.Pos.Maui;

public partial class DailyAccountPage : ContentPage
{
    public DailyAccountPage(DailyAccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
