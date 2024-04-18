namespace TheCardEditor.Shared;

public static class EnumerableExtensions
{
    public static IEnumerable<T> AppendRange<T>(this IEnumerable<T> source, IEnumerable<T> target)
    {
        foreach (var item in source) yield return item;
        foreach (var item in target) yield return item;
    }

    public static List<T> PushIf<T>(this List<T> list, T item, Func<T, bool> condition)
    {
        if (condition(item)) return list.Push(item);
        return list;
    }

    public static List<T> Push<T>(this List<T> list, T item)
    {
        list.Add(item);
        return list;
    }

    public static string Concat<T>(this IEnumerable<T> objects, string separator)
    {
        return string.Join(separator, objects);
    }

    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}
