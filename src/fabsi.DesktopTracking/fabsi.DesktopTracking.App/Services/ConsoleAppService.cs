using System.Runtime.InteropServices;

namespace fabsi.DesktopTracking.App.Services;

public interface IConsoleAppService
{
    void ValidateBackgroundApp();
    void RunService(Func<CancellationToken, Task> method, CancellationToken ct = default);
}

public class ConsoleAppService : IConsoleAppService
{
    [DllImport( "user32.dll" )]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    public const int SW_SHOWMINIMIZED = 2;

    public ConsoleAppService()
    {

    }

    public void ValidateBackgroundApp()
    {
        IntPtr winHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        ShowWindow(winHandle, SW_SHOWMINIMIZED);
    }

    private List<Task> RunningTasks { get; set; } = new();

    public void RunService(Func<CancellationToken, Task> method, CancellationToken ct = default)
    {
        RunningTasks.Add(Task.Factory.StartNew(async () => await method(ct), ct));
    }
}