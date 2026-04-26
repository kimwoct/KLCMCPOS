using KLCMC.Pos.Core.ViewModels;

namespace KLCMC.Pos.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
