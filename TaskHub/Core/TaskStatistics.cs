using TaskHub.Models;

namespace TaskHub.Core;

public sealed class TaskStatistics
{
    public int Total { get; init; }

    public int Completed { get; init; }

    public int Overdue { get; init; }

    public Dictionary<Priority, int> ByPriority { get; init; } = new();
}
