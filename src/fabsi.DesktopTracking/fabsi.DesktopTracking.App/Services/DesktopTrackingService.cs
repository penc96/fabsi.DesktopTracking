using System.Timers;
using Timer = System.Timers.Timer;

namespace fabsi.DesktopTracking.App.Services;

public interface IDesktopTrackingService
{
    Task TrackVirtualDesktopsAsync(CancellationToken ct = default);
}

public class DesktopTrackingService : IDesktopTrackingService
{
    private Timer ExportTimer { get; }

    private readonly IJsonDataService _service;
    private readonly IVirtualDesktopService _virtualDesktopService;

    public DesktopTrackingService(IJsonDataService service,
                                  IVirtualDesktopService virtualDesktopService,
                                  TimeSpan exportInterval)
    {
        _service = service;
        _virtualDesktopService = virtualDesktopService;
        ExportTimer = new Timer(exportInterval.TotalMilliseconds);
    }

    public async Task TrackVirtualDesktopsAsync(CancellationToken ct = default)
    {
        _virtualDesktopService.Start();

        ExportTimer.Elapsed += Export;
        ExportTimer.Enabled = true;
        ExportTimer.Start();

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct);
        }
        _virtualDesktopService.Stop();
        ExportTimer.Stop();
        ExportTimer.Dispose();
    }

    private void Export(object? sender, ElapsedEventArgs e)
    {
        Console.WriteLine($"{nameof(DesktopTrackingService)} :: Export data");
        if (!_virtualDesktopService.IsStarted)
        {
            Console.WriteLine($"{nameof(DesktopTrackingService)} :: VirtualDesktopService is not started yet.");
            return;
        }

        _service.ExportData(_virtualDesktopService.TrackingData);
    }
}