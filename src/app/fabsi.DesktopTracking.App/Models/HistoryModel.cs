namespace fabsi.DesktopTracking.App.Models;

public class HistoryModel<TFieldType>
{
    public TFieldType Value { get; set; } = default!;
    public DateTime TimeStamp { get; set; }
}