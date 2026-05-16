namespace TaskHub.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Priority Priority { get; set; }

    public DateTime Deadline { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.New;

    public bool IsCompleted => Status == TaskItemStatus.Done;

    public bool IsOverdue(DateTime now) =>
        !IsCompleted && Deadline < now;

    public override string ToString()
    {
        var overdueMark = Status != TaskItemStatus.Done && Deadline < DateTime.Now ? " [OVERDUE]" : "";
        return $"[{Id:N}] {Name} | {Priority} | {Status} | до {Deadline:dd.MM.yyyy HH:mm}{overdueMark}\n    {Description}";
    }
}
