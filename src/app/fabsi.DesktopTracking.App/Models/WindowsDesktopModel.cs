namespace fabsi.DesktopTracking.App.Models;

public class WindowsDesktopModel
{
    public Guid DesktopId { get; set; }
    public List<HistoryModel<string>> NameHistory { get; set; } = new();
    public List<TimerHistoryModel> TimerHistory { get; set; } = new();
}