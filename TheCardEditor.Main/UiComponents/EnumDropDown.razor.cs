using Microsoft.AspNetCore.Components;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.UiComponents;

public partial class EnumDropDown<T> : ComponentBase where T : Enum
{
    private T? _selectedItem;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = new();

    [Parameter]
    public Func<T, bool> Filter { get; set; } = _ => true;

    [Parameter]
    public string? Label { get; set; }

    [Parameter] public Func<T?, string> ErrorValidator { get; set; } = _ => "";

    public IEnumerable<T>? Items { get; set; }
    public List<T> FilteredItems => Items?.Where(i => Filter(i)).ToList() ?? new();
    private string _error = "";

    protected override void OnInitialized()
    {
        Items = Enum.GetValues(typeof(T)).Cast<T>();
        base.OnInitialized();
    }

    public async Task SelectedEnumChanged(ChangeEventArgs args)
    {
        var result = (args.Value?.ToString() ?? "").GetEnumValue<T>(out var success);
        if (!success) return;
        SelectedItem = result;
        _error = ErrorValidator(SelectedItem);
        await SelectedItemChanged.InvokeAsync(SelectedItem);
    }

    protected override async Task OnParametersSetAsync()
    {
        _error = ErrorValidator(SelectedItem);
        if (SelectedItem == null || SelectedItem.Equals(_selectedItem)) return;
        _selectedItem = SelectedItem;
        await SelectedItemChanged.InvokeAsync(_selectedItem);
    }

    [Parameter]
    public T? SelectedItem { get; set; }

    [Parameter]
    public EventCallback<T> SelectedItemChanged { get; set; }
}
