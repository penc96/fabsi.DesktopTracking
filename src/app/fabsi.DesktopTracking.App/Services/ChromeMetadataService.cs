using System.Diagnostics;
using System.Windows.Automation;

namespace fabsi.DesktopTracking.App.Services;

public interface IChromeMetadataService
{

}

public class ChromeMetadataService
{
    public ChromeMetadataService()
    {

        Console.WriteLine($"=== Begin Process List === ");
        var processes = Process.GetProcesses();
        var procsWithTitle = processes.Where(x => !string.IsNullOrWhiteSpace(x.MainWindowTitle)).ToList();

        foreach (var process in procsWithTitle)
        {
            Console.WriteLine($"Process: '{process.ProcessName}' | Title: '{process.MainWindowTitle}'");
        }
        var chromeProc = procsWithTitle.First(x => x.ProcessName.Contains("chrome"));
        AutomationElement root = AutomationElement.FromHandle(chromeProc.MainWindowHandle);
        Condition conditionNewTab = new PropertyCondition(AutomationElement.NameProperty, "Neuer Tab");
        AutomationElement elemNewTab = root.FindFirst(TreeScope.Descendants, conditionNewTab);
        TreeWalker treeWalker = TreeWalker.ControlViewWalker;
        AutomationElement element = treeWalker.GetParent(elemNewTab);
        Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
        var items = element.FindAll(TreeScope.Children, condTabItem);
        var chromeTabNames = new List<string>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            chromeTabNames.Add(item.Current.Name);
        }

        Console.WriteLine($"=== End Process List === ");

    }
}