using System.Windows;
using KLCMC.Pos.App.Services;
using KLCMC.Pos.App.ViewModels;

namespace KLCMC.Pos.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new PosDllPrinterService());
    }
}
