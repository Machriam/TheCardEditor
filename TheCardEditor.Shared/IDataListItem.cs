namespace TheCardEditor.Shared;

public class StringDataListItem : IDataListItem
{
    public string? GetText { get; set; }
}

public static class DataListItemExtensions
{
    public static T GetItem<T>(this IDataListItem item) where T : IDataListItem
    {
        return (T)item;
    }
}

public interface IDataListItem
{
    string? GetText { get; }
}
