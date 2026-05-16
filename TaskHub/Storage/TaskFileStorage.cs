using System.Text.Json;
using TaskHub.Models;

namespace TaskHub.Storage;

public sealed class TaskFileStorage : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _filePath;
    private bool _disposed;

    public TaskFileStorage(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));
        _filePath = filePath;
    }

    public async Task SaveAsync(IEnumerable<TaskItem> tasks, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var stream = new FileStream(
                _filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await JsonSerializer.SerializeAsync(stream, tasks.ToList(), JsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new InvalidOperationException($"Не удалось сохранить задачи в файл: {_filePath}", ex);
        }
    }

    public async Task<List<TaskItem>> LoadAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!File.Exists(_filePath))
            throw new FileNotFoundException("Файл с задачами не найден.", _filePath);

        try
        {
            await using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            var tasks = await JsonSerializer.DeserializeAsync<List<TaskItem>>(stream, JsonOptions, cancellationToken);
            return tasks ?? new List<TaskItem>();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new InvalidOperationException($"Не удалось загрузить задачи из файла: {_filePath}", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TaskFileStorage));
    }
}
