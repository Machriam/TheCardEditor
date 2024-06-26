﻿using Microsoft.AspNetCore.Components;
using TheCardEditor.Shared.Features.CardEditor;

namespace TheCardEditor.Main.Features.CardEditor;

public partial class FilterChainSelection
{
    [Parameter]
    public EventCallback<ImageFilterModel[]> FilterHasChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = new();

    private class FilterSelectionModel
    {
        public ImageFilterType Type { get; set; } = ImageFilterType.NA;
    }

    private readonly List<FilterSelectionModel> _selectedFilters = [new()];

    public async Task FilterChanged(ImageFilterType type, int index)
    {
        _selectedFilters[index].Type = type;
        if (_selectedFilters.Last().Type != ImageFilterType.NA) _selectedFilters.Add(new());
        if (_selectedFilters.Count > 1 && _selectedFilters[^1].Type == ImageFilterType.NA && _selectedFilters[^2].Type == ImageFilterType.NA)
        {
            _selectedFilters.RemoveAt(_selectedFilters.Count - 1);
        }
        var filters = _selectedFilters
            .Where(sf => sf.Type != ImageFilterType.NA)
            .Select(sf => new ImageFilterModel().For().InvokeFilter(sf.Type.ToString()))
            .ToArray();
        await FilterHasChanged.InvokeAsync(filters);
    }
}
