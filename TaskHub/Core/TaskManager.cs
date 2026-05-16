using TaskHub.Models;

namespace TaskHub.Core;

public delegate void TasksChangedHandler();

public class TaskManager
{
    private readonly List<TaskItem> _tasks = new();
    private readonly object _sync = new();

    public event TasksChangedHandler? TasksChanged;

    public IReadOnlyList<TaskItem> GetAll()
    {
        lock (_sync)
            return _tasks.ToList();
    }

    public void Add(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        lock (_sync)
            _tasks.Add(task);
        OnTasksChanged();
    }

    public bool Remove(Guid id)
    {
        lock (_sync)
        {
            var index = _tasks.FindIndex(t => t.Id == id);
            if (index < 0)
                return false;
            _tasks.RemoveAt(index);
        }

        OnTasksChanged();
        return true;
    }

    public bool Update(Guid id, Action<TaskItem> updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        lock (_sync)
        {
            var task = CollectionHelper.FirstOrDefault(_tasks, t => t.Id == id);
            if (task is null)
                return false;
            updateAction(task);
        }

        OnTasksChanged();
        return true;
    }

    public TaskItem? FindById(Guid id)
    {
        lock (_sync)
            return CollectionHelper.FirstOrDefault(_tasks, t => t.Id == id);
    }

    public List<TaskItem> Filter(Func<TaskItem, bool> predicate) =>
        CollectionHelper.WhereMatch(GetAll(), predicate);

    public List<TaskItem> GetCompleted() =>
        Filter(t => t.IsCompleted);

    public List<TaskItem> GetIncomplete() =>
        Filter(t => !t.IsCompleted);

    public List<TaskItem> GetHighPriority() =>
        Filter(t => t.Priority == Priority.High);

    public List<TaskItem> SearchByName(string namePart)
    {
        var query = namePart.Trim();
        return Filter(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public List<TaskItem> SearchByStatus(TaskItemStatus status) =>
        Filter(t => t.Status == status);

    public List<TaskItem> SearchByPriority(Priority priority) =>
        Filter(t => t.Priority == priority);

    public TaskStatistics GetStatistics(DateTime now)
    {
        lock (_sync)
        {
            var byPriority = new Dictionary<Priority, int>
            {
                [Priority.Low] = 0,
                [Priority.Medium] = 0,
                [Priority.High] = 0
            };

            foreach (var task in _tasks)
                byPriority[task.Priority]++;

            return new TaskStatistics
            {
                Total = _tasks.Count,
                Completed = _tasks.Count(t => t.IsCompleted),
                Overdue = _tasks.Count(t => t.IsOverdue(now)),
                ByPriority = byPriority
            };
        }
    }

    public void ReplaceAll(IEnumerable<TaskItem> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        lock (_sync)
        {
            _tasks.Clear();
            _tasks.AddRange(tasks);
        }

        OnTasksChanged();
    }

    public List<TaskItem> GetOverdue(DateTime now) =>
        Filter(t => t.IsOverdue(now));

    private void OnTasksChanged() => TasksChanged?.Invoke();
}
