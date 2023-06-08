
using fabsi.DesktopTracking.App.Services;
using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Controls;
using WindowsDesktop;
using WinRT;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        IConsoleAppService consoleService = new ConsoleAppService();
        // consoleService.ValidateBackgroundApp();

        var ctSource = new CancellationTokenSource();

        IJsonDataService jsonDataService = new JsonDataService();
        IVirtualDesktopService virtualDesktopService = new VirtualDesktopService(jsonDataService, TimeSpan.FromSeconds(5));
        IDesktopTrackingService trackingService = new DesktopTrackingService(jsonDataService, virtualDesktopService, TimeSpan.FromSeconds(5));
        consoleService.RunService(trackingService.TrackVirtualDesktopsAsync, ctSource.Token);

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            ctSource.Cancel();
        };
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            ctSource.Cancel();
        };

        Console.Write("Press any key to exit...");
        Console.ReadKey();
    }
}
