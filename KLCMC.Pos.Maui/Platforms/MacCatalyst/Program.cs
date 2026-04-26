using Foundation;
using UIKit;

namespace KLCMC.Pos.Maui;

[Register("AppDelegate")]
public class Program : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(Program));
    }
}
