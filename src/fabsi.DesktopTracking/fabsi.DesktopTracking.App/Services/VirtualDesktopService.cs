using fabsi.DesktopTracking.App.Models;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Windows.Media.Core;
using WindowsDesktop;

namespace fabsi.DesktopTracking.App.Services;

public interface IVirtualDesktopService
{
    bool IsStarted { get; set; }
    void Start();
    void Stop();
    DesktopTrackingModel TrackingData { get; }
}

public class VirtualDesktopService : IVirtualDesktopService
{
    private readonly IJsonDataService _jsonDataService;
    public DesktopTrackingModel TrackingData { get; private set; } = new();
    public bool IsStarted { get; set; }

    public DateTime From { get; set; }
    public Guid CurrentDesktopId { get; set; }
    protected TimeSpan Threshold { get; }

    public VirtualDesktopService(IJsonDataService jsonDataService, TimeSpan threshold)
    {
        Threshold = threshold;
        _jsonDataService = jsonDataService;
        InitializeDesktops();
    }

    private void InitializeDesktops()
    {
        var existingData = _jsonDataService.ImportData();
        if (existingData != null)
            TrackingData = existingData;

        var desktops = VirtualDesktop.GetDesktops();
        foreach (var desktop in desktops)
        {
            var existing = TrackingData.Desktops.FirstOrDefault(x => x.DesktopId == desktop.Id);
            if (existing == null)
            {
                TrackingData.Desktops.Add(new WindowsDesktopModel
                {
                    DesktopId = desktop.Id,
                    NameHistory = new List<HistoryModel<string>>
                    {
                        new HistoryModel<string>
                        {
                            Value = desktop.Name,
                            TimeStamp = DateTime.UtcNow,
                        },
                    },
                });
            }
            else
            {
                var lastEntry = existing.NameHistory.MaxBy(x => x.TimeStamp);
                if (lastEntry == null || lastEntry.Value != desktop.Name)
                {
                    existing.NameHistory.Add(new HistoryModel<string>
                    {
                        Value = desktop.Name,
                        TimeStamp = DateTime.UtcNow,
                    });
                }
            }
        }

        CurrentDesktopId = VirtualDesktop.Current.Id;
        From = DateTime.UtcNow;
        // _windowsMetadataService.StartByDesktopId(CurrentDesktopId);
    }

    public void Start()
    {
        VirtualDesktop.Created += OnCreated;
        VirtualDesktop.Destroyed += OnDestroyed;
        VirtualDesktop.CurrentChanged += OnCurrentChanged;
        VirtualDesktop.Renamed += OnRenamed;
        VirtualDesktop.Moved += OnMoved;
        IsStarted = true;
    }

    public void Stop()
    {
        VirtualDesktop.Created -= OnCreated;
        VirtualDesktop.Destroyed -= OnDestroyed;
        VirtualDesktop.CurrentChanged -= OnCurrentChanged;
        VirtualDesktop.Renamed -= OnRenamed;
        VirtualDesktop.Moved -= OnMoved;
        IsStarted = false;
    }

    private void OnCreated(object? sender, VirtualDesktop eventArgs)
    {
        TrackingData.Desktops.Add(new WindowsDesktopModel
        {
            DesktopId = eventArgs.Id,
            NameHistory = new List<HistoryModel<string>>
            {
                new HistoryModel<string>
                {
                    Value = eventArgs.Name,
                    TimeStamp = DateTime.UtcNow,
                },
            },
        });
    }

    private void OnDestroyed(object? sender, VirtualDesktopDestroyEventArgs eventArgs)
    {
        // TODO: what to do, if desktop is destroyed? leave data?
    }

    private void OnCurrentChanged(object? sender, VirtualDesktopChangedEventArgs eventArgs)
    {
        var now = DateTime.UtcNow;
        var diff = now - From;
        if (diff.TotalSeconds >= Threshold.TotalSeconds)
        {
            var lastDesktop = TrackingData.Desktops.First(x => x.DesktopId == CurrentDesktopId);
            lastDesktop.TimerHistory.Add(new TimerHistoryModel
            {
                // Metadata = _windowMetadataService.GetMetadataByDesktopId(CurrentDesktopId);
                From = From,
                To = now,
            });
        }

        CurrentDesktopId = eventArgs.NewDesktop.Id;
        // _windowMetadataService.RestartByDesktopId(CurrentDesktopId);
        From = now;
    }

    private void OnRenamed(object? sender, VirtualDesktopRenamedEventArgs eventArgs)
    {
        var targetDesktop = TrackingData.Desktops.First(x => x.DesktopId == eventArgs.Source.Id);
        targetDesktop.NameHistory.Add(new HistoryModel<string>
        {
            Value = eventArgs.NewName,
            TimeStamp = DateTime.UtcNow,
        });
    }

    private void OnMoved(object? sender, VirtualDesktopMovedEventArgs eventArgs)
    {
        Console.WriteLine($"Desktop: Moved desktop '{eventArgs.Source.Name}' from index '{eventArgs.OldIndex}' to index '{eventArgs.NewIndex}'");
    }
}