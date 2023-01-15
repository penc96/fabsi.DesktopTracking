using fabsi.DesktopTracking.App.Models;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace fabsi.DesktopTracking.App.Services;

public interface IJsonDataService
{
    void ExportData(DesktopTrackingModel desktopTrackingData);
    DesktopTrackingModel? ImportData();
}

public class JsonDataService : IJsonDataService
{
    public void ExportData(DesktopTrackingModel desktopTrackingData)
    {
        try
        {
            string filenameSnapshot = CreateFilename();
            string dataSnapshot = JsonConvert.SerializeObject(desktopTrackingData, Formatting.Indented);
            string snapshotDirectory = CreateSnapshotDirectory();
            string fullSnapshotPath = Path.Combine(snapshotDirectory, filenameSnapshot);
            File.WriteAllText(fullSnapshotPath, dataSnapshot);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public DesktopTrackingModel? ImportData()
    {
        try
        {
            string snapshotDirectory = CreateSnapshotDirectory();
            string filename = CreateFilename();
            string fullSnapshotPath = Path.Combine(snapshotDirectory, filename);
            if (!File.Exists(fullSnapshotPath))
                return null;
            return JsonConvert.DeserializeObject<DesktopTrackingModel>(File.ReadAllText(fullSnapshotPath));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private string CreateSnapshotDirectory()
    {
        var date = DateTime.UtcNow;
        string currentDateSnapshot = date.ToString("yyyyMMdd");
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string snapshotDirectory = Path.Combine(currentDirectory, "snapshots", currentDateSnapshot);
        if (!Directory.Exists(snapshotDirectory))
            Directory.CreateDirectory(snapshotDirectory);
        return snapshotDirectory;
    }

    private string CreateFilename()
    {
        var date = DateTime.UtcNow;
        short minutesFactor = (short)((double)date.Minute / 15);
        string dateSnapshot = date.ToString($"yyyyMMdd-hh{(minutesFactor * 15).ToString().PadLeft(2, '0')}");
        return $"desktop-tracking-data.{dateSnapshot}.json";
    }
}