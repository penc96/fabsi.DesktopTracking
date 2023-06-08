namespace fabsi.DesktopTracking.App.Models;

public class TimerHistoryModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}