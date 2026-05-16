using TaskHub.Background;
using TaskHub.Core;
using TaskHub.Models;
using TaskHub.Storage;

namespace TaskHub.UI;

public delegate void MenuAction();

public sealed class MenuApp : IDisposable
{
    private readonly TaskManager _taskManager;
    private readonly TaskFileStorage _storage;
    private readonly DeadlineMonitor _deadlineMonitor;
    private readonly Dictionary<string, MenuAction> _menuActions;
    private bool _disposed;

    public MenuApp(TaskManager taskManager, string dataFilePath)
    {
        _taskManager = taskManager;
        _storage = new TaskFileStorage(dataFilePath);
        _deadlineMonitor = new DeadlineMonitor(_taskManager, TimeSpan.FromSeconds(5));
        _deadlineMonitor.OverdueTasksDetected += OnOverdueTasksDetected;

        _menuActions = new Dictionary<string, MenuAction>(StringComparer.Ordinal)
        {
            ["1"] = CreateTask,
            ["2"] = ShowAllTasks,
            ["3"] = ShowCompleted,
            ["4"] = ShowIncomplete,
            ["5"] = ShowHighPriority,
            ["6"] = EditTask,
            ["7"] = DeleteTask,
            ["8"] = SearchTasks,
            ["9"] = ShowStatistics,
            ["10"] = SaveTasks,
            ["11"] = LoadTasks,
            ["0"] = () => { }
        };
    }

    public void Run()
    {
        _deadlineMonitor.Start();
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var running = true;
        while (running)
        {
            PrintMenu();
            Console.Write("Выберите пункт: ");
            var choice = Console.ReadLine()?.Trim() ?? string.Empty;

            if (choice == "0")
            {
                running = false;
                continue;
            }

            if (_menuActions.TryGetValue(choice, out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                ConsoleHelper.Pause();
            }
            else
            {
                Console.WriteLine("Неизвестный пункт меню.");
            }
        }
    }

    private static void PrintMenu()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("TaskHub — менеджер задач");
        Console.WriteLine(" 1.  Создать задачу");
        Console.WriteLine(" 2.  Показать все задачи");
        Console.WriteLine(" 3.  Показать выполненные");
        Console.WriteLine(" 4.  Показать невыполненные");
        Console.WriteLine(" 5.  Показать с высоким приоритетом");
        Console.WriteLine(" 6.  Редактировать задачу");
        Console.WriteLine(" 7.  Удалить задачу");
        Console.WriteLine(" 8.  Поиск задач");
        Console.WriteLine(" 9.  Статистика");
        Console.WriteLine("10.  Сохранить в файл");
        Console.WriteLine("11.  Загрузить из файла");
        Console.WriteLine(" 0.  Выход");
    }

    private void CreateTask()
    {
        ConsoleHelper.WriteHeader("Создание задачи");
        var task = new TaskItem
        {
            Name = ConsoleHelper.ReadLine("Название: "),
            Description = ConsoleHelper.ReadLine("Описание: "),
            Priority = ConsoleHelper.ReadPriority(),
            Deadline = ConsoleHelper.ReadDeadline(),
            Status = ConsoleHelper.ReadStatus()
        };

        _taskManager.Add(task);
        Console.WriteLine("Задача создана.");
    }

    private void ShowAllTasks()
    {
        ConsoleHelper.WriteHeader("Все задачи");
        ConsoleHelper.WriteTasks(_taskManager.GetAll());
    }

    private void ShowCompleted()
    {
        ConsoleHelper.WriteHeader("Выполненные задачи");
        ConsoleHelper.WriteTasks(_taskManager.GetCompleted());
    }

    private void ShowIncomplete()
    {
        ConsoleHelper.WriteHeader("Невыполненные задачи");
        ConsoleHelper.WriteTasks(_taskManager.GetIncomplete());
    }

    private void ShowHighPriority()
    {
        ConsoleHelper.WriteHeader("Задачи с высоким приоритетом");
        ConsoleHelper.WriteTasks(_taskManager.GetHighPriority());
    }

    private void EditTask()
    {
        ConsoleHelper.WriteHeader("Редактирование задачи");
        var id = ConsoleHelper.ReadTaskId(_taskManager);
        if (id is null)
            return;

        var updated = _taskManager.Update(id.Value, task =>
        {
            Console.WriteLine("Оставьте поле пустым, чтобы не менять его.");
            var name = ConsoleHelper.ReadLine("Новое название: ", required: false);
            if (!string.IsNullOrWhiteSpace(name))
                task.Name = name;

            var description = ConsoleHelper.ReadLine("Новое описание: ", required: false);
            if (!string.IsNullOrWhiteSpace(description))
                task.Description = description;

            Console.Write("Изменить приоритет? (y/n): ");
            if (Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
                task.Priority = ConsoleHelper.ReadPriority();

            Console.Write("Изменить статус? (y/n): ");
            if (Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
                task.Status = ConsoleHelper.ReadStatus();
        });

        Console.WriteLine(updated ? "Задача обновлена." : "Задача не найдена.");
    }

    private void DeleteTask()
    {
        ConsoleHelper.WriteHeader("Удаление задачи");
        var id = ConsoleHelper.ReadTaskId(_taskManager);
        if (id is null)
            return;

        var removed = _taskManager.Remove(id.Value);
        Console.WriteLine(removed ? "Задача удалена." : "Задача не найдена.");
    }

    private void SearchTasks()
    {
        ConsoleHelper.WriteHeader("Поиск задач");
        Console.WriteLine("1 - по названию, 2 - по статусу, 3 - по приоритету");
        var mode = Console.ReadLine()?.Trim();

        List<TaskItem> results = mode switch
        {
            "1" => _taskManager.SearchByName(ConsoleHelper.ReadLine("Часть названия: ")),
            "2" => _taskManager.SearchByStatus(ConsoleHelper.ReadStatus()),
            "3" => _taskManager.SearchByPriority(ConsoleHelper.ReadPriority()),
            _ => new List<TaskItem>()
        };

        if (mode is not ("1" or "2" or "3"))
            Console.WriteLine("Неверный режим поиска.");
        else
            ConsoleHelper.WriteTasks(results);
    }

    private void ShowStatistics() => ConsoleHelper.WriteStatistics(_taskManager.GetStatistics(DateTime.Now));

    private void SaveTasks() => SaveTasksAsync().GetAwaiter().GetResult();

    private async Task SaveTasksAsync()
    {
        ConsoleHelper.WriteHeader("Сохранение");
        await _storage.SaveAsync(_taskManager.GetAll());
        Console.WriteLine("Задачи сохранены.");
    }

    private void LoadTasks() => LoadTasksAsync().GetAwaiter().GetResult();

    private async Task LoadTasksAsync()
    {
        ConsoleHelper.WriteHeader("Загрузка");
        var tasks = await _storage.LoadAsync();
        _taskManager.ReplaceAll(tasks);
        _deadlineMonitor.ResetNotifications();
        Console.WriteLine($"Загружено задач: {tasks.Count}.");
    }

    private static void OnOverdueTasksDetected(IReadOnlyList<TaskItem> overdueTasks)
    {
        Console.WriteLine();
        Console.WriteLine("!!! УВЕДОМЛЕНИЕ: просроченные задачи !!!");
        foreach (var task in overdueTasks)
            Console.WriteLine($"  - {task.Name} (дедлайн: {task.Deadline:dd.MM.yyyy HH:mm})");
        Console.WriteLine();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _deadlineMonitor.OverdueTasksDetected -= OnOverdueTasksDetected;
        _deadlineMonitor.Dispose();
        _storage.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
