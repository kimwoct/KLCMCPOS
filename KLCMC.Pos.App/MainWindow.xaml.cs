using KLCMC.Pos.Core.ViewModels;
using System.Windows;

namespace KLCMC.Pos.App;

public partial class MainWindow : Window
{
    public MainViewModel MainViewModel { get; }
    public DailyAccountViewModel DailyAccountViewModel { get; }

    public MainWindow(MainViewModel mainViewModel, DailyAccountViewModel dailyAccountViewModel)
    {
        MainViewModel = mainViewModel;
        DailyAccountViewModel = dailyAccountViewModel;
        InitializeComponent();
        DataContext = this;
    }

    private void DailyAccountTab_GotFocus(object sender, RoutedEventArgs e)
    {
        DailyAccountViewModel.Load();
    }
}
