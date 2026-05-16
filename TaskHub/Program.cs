using TaskHub.Core;
using TaskHub.UI;

namespace TaskHub;

internal static class Program
{
    private static readonly string DataFilePath = Path.Combine(
        AppContext.BaseDirectory,
        "tasks.json");

    private static void Main()
    {
        Console.Title = "TaskHub";
        var taskManager = new TaskManager();

        using var app = new MenuApp(taskManager, DataFilePath);
        app.Run();

        Console.WriteLine("До свидания!");
    }
}
