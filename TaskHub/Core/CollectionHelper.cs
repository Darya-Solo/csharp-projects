namespace TaskHub.Core;

public static class CollectionHelper
{
    public static List<T> WhereMatch<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var result = new List<T>();
        foreach (var item in source)
        {
            if (predicate(item))
                result.Add(item);
        }

        return result;
    }

    public static T? FirstOrDefault<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
                return item;
        }

        return default;
    }
}
