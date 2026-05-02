namespace Generics;

public class Repository<T> where T : IEntity
{
    private readonly Dictionary<int, T> _storage = new();

    public int Count => _storage.Count;

    public void Add(T item)
    {
        if (_storage.ContainsKey(item.Id))
            throw new InvalidOperationException($"Элемент с Id={item.Id} уже существует.");

        _storage[item.Id] = item;
    }

    public bool Remove(int id) => _storage.Remove(id);

    public T? GetById(int id) => _storage.TryGetValue(id, out var item) ? item : default;

    public IReadOnlyList<T> GetAll() => _storage.Values.ToList().AsReadOnly();

    public IReadOnlyList<T> Find(Predicate<T> predicate)
    {
        var result = new List<T>();
        foreach (var item in _storage.Values)
        {
            if (predicate(item))
                result.Add(item);
        }
        return result.AsReadOnly();
    }
}
