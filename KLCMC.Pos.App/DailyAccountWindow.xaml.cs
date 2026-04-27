using System.Windows;
using KLCMC.Pos.Core.ViewModels;

namespace KLCMC.Pos.App;

public partial class DailyAccountWindow : Window
{
    public DailyAccountWindow(DailyAccountViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
