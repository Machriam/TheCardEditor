using Microsoft.AspNetCore.Components;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Pages.Components;

public class DataListEntry
{
    public DataListEntry(IDataListItem? selectedItem, string? selectedText)
    {
        SelectedItem = selectedItem;
        SelectedText = selectedText;
    }

    public IDataListItem? SelectedItem { get; }
    public string? SelectedText { get; }
}

public partial class DataList : ComponentBase
{
    private readonly string _guid = Guid.NewGuid().ToString("N");

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public Func<IDataListItem, bool>? InputValidator { get; set; }

    [Parameter]
    public Func<string>? SelectedItemSelector { get; set; }

    [Parameter]
    public string? LabelText { get; set; }

    private IEnumerable<IDataListItem>? _items;
    private IEnumerable<IDataListItem>? _filteredItems;

    protected override void OnParametersSet()
    {
        if (_items == Items) return;
        _items = Items;
        if (InputValidator != null) _filteredItems = Items.Where(InputValidator);
        else _filteredItems = Items;
        if (SelectedItemSelector != null) SelectedText = SelectedItemSelector.Invoke();
    }

    [Parameter]
    public IEnumerable<IDataListItem> Items { get; set; } = new List<IDataListItem>();

    [Parameter]
    public EventCallback<DataListEntry> InputHasChanged { get; set; }

    [Parameter]
    public EventCallback<IDataListItem?> SelectedItemChanged { get; set; }

    private string? _selectedText;

    public string? SelectedText
    {
        get => _selectedText; set
        {
            if (value == _selectedText) return;
            _selectedText = value;
            var selectedItem = Items?.FirstOrDefault(i => i.GetText?.Replace(" ", "") == value?.Replace(" ", ""));
            InputHasChanged.InvokeAsync(new DataListEntry(selectedItem, value));
            SelectedItemChanged.InvokeAsync(selectedItem);
        }
    }
}
