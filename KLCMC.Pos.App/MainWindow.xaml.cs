using System.Windows;
using KLCMC.Pos.Core.ViewModels;
using KLCMC.Pos.Printer.Windows;

namespace KLCMC.Pos.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new PosDllPrinterService());
    }
}
