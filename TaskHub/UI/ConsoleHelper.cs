using TaskHub.Core;
using TaskHub.Models;

namespace TaskHub.UI;

public static class ConsoleHelper
{
    public static void WriteHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', title.Length + 4));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', title.Length + 4));
    }

    public static void WriteTasks(IEnumerable<TaskItem> tasks, string emptyMessage = "Задач не найдено.")
    {
        var list = tasks.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine(emptyMessage);
            return;
        }

        for (var i = 0; i < list.Count; i++)
            Console.WriteLine($"{i + 1}. {list[i]}");
    }

    public static void WriteStatistics(TaskStatistics stats)
    {
        WriteHeader("Статистика");
        Console.WriteLine($"Всего задач:      {stats.Total}");
        Console.WriteLine($"Выполнено:        {stats.Completed}");
        Console.WriteLine($"Просрочено:       {stats.Overdue}");
        Console.WriteLine("По приоритетам:");
        foreach (var pair in stats.ByPriority)
            Console.WriteLine($"  {pair.Key,-8}: {pair.Value}");
    }

    public static string ReadLine(string prompt, bool required = true)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!required || !string.IsNullOrWhiteSpace(input))
                return input;
            Console.WriteLine("Значение не может быть пустым.");
        }
    }

    public static Priority ReadPriority()
    {
        while (true)
        {
            Console.WriteLine("Приоритет: 1 - Low, 2 - Medium, 3 - High");
            var input = Console.ReadLine()?.Trim();
            switch (input)
            {
                case "1": return Priority.Low;
                case "2": return Priority.Medium;
                case "3": return Priority.High;
                default:
                    Console.WriteLine("Неверный приоритет.");
                    break;
            }
        }
    }

    public static TaskItemStatus ReadStatus()
    {
        while (true)
        {
            Console.WriteLine("Статус: 1 - New, 2 - InProgress, 3 - Done");
            var input = Console.ReadLine()?.Trim();
            if (input is "1" or "2" or "3")
            {
                return input switch
                {
                    "1" => TaskItemStatus.New,
                    "2" => TaskItemStatus.InProgress,
                    "3" => TaskItemStatus.Done,
                    _ => TaskItemStatus.New
                };
            }

            Console.WriteLine("Неверный статус.");
        }
    }

    public static DateTime ReadDeadline()
    {
        while (true)
        {
            Console.Write("Дедлайн (дд.мм.гггг чч:мм): ");
            var input = Console.ReadLine()?.Trim();
            if (DateTime.TryParse(input, out var deadline))
                return deadline;

            Console.WriteLine("Неверный формат даты. Пример: 25.12.2026 18:00");
        }
    }

    public static Guid? ReadTaskId(TaskManager manager)
    {
        var tasks = manager.GetAll();
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return null;
        }

        WriteTasks(tasks);
        Console.Write("Введите номер задачи из списка: ");
        if (!int.TryParse(Console.ReadLine(), out var index) || index < 1 || index > tasks.Count)
        {
            Console.WriteLine("Неверный номер.");
            return null;
        }

        return tasks[index - 1].Id;
    }

    public static void Pause() => ReadLine("\nНажмите Enter для продолжения...", required: false);
}
