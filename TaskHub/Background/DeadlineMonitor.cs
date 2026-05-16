using TaskHub.Core;
using TaskHub.Models;

namespace TaskHub.Background;

public delegate void OverdueTasksDetectedHandler(IReadOnlyList<TaskItem> overdueTasks);

public sealed class DeadlineMonitor : IDisposable
{
    private readonly TaskManager _taskManager;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _cts = new();
    private Task? _worker;
    private bool _disposed;
    private readonly HashSet<Guid> _notifiedIds = new();

    public event OverdueTasksDetectedHandler? OverdueTasksDetected;

    public DeadlineMonitor(TaskManager taskManager, TimeSpan interval)
    {
        _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        _interval = interval <= TimeSpan.Zero ? TimeSpan.FromSeconds(5) : interval;
    }

    public void Start()
    {
        ThrowIfDisposed();
        if (_worker is not null)
            return;

        _worker = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
    }

    private async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, token);
                CheckDeadlines();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private void CheckDeadlines()
    {
        var now = DateTime.Now;
        var overdue = _taskManager.GetOverdue(now);

        lock (_notifiedIds)
        {
            var fresh = overdue.Where(t => !_notifiedIds.Contains(t.Id)).ToList();
            foreach (var task in overdue)
                _notifiedIds.Add(task.Id);

            if (fresh.Count > 0)
                OverdueTasksDetected?.Invoke(fresh);
        }
    }

    public void ResetNotifications() => _notifiedIds.Clear();

    public void Dispose()
    {
        if (_disposed)
            return;

        _cts.Cancel();
        try
        {
            _worker?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // ignored on shutdown
        }

        _cts.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DeadlineMonitor));
    }
}
