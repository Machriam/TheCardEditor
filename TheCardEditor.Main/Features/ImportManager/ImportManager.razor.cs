using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using TheCardEditor.SheetComponent;

namespace TheCardEditor.Main.Features.ImportManager;

public partial class ImportManager : IDisposable
{
    private class ImportSheetModel : AbstractSheetModel
    {
        [SheetMetaData(HeaderName = "Card Name")]
        public string CardName { get; set; } = "";

        [SheetMetaData(HeaderName = "Tags")]
        [JsonExtensionData]
        public Dictionary<string, object> TagTexts { get; set; } = [];
    }

    private const string ImportManagerSheet = nameof(ImportManagerSheet);
    [Inject] private ISheetViewFactory SheetViewFactory { get; set; } = default!;
    [Inject] private ServiceAccessor<TemplateService> TemplateService { get; set; } = default!;
    [Inject] private ApplicationStorage Application { get; set; } = default!;
    [Inject] private ServiceAccessor<CardService> CardService { get; set; } = default!;
    private IXSheetView _sheetView = default!;
    private IReadOnlyDictionary<int, string> _templateById = new Dictionary<int, string>();
    private int? _selectedTemplate;
    private static readonly string s_guid = new Guid().ToString();

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return Task.CompletedTask;
        Initialize();
        return base.OnAfterRenderAsync(firstRender);
    }

    public void Initialize()
    {
        if (Application.SelectedCardSet == null) return;
        _sheetView = SheetViewFactory.CreateSheet(this, ImportManagerSheet);
        _templateById = TemplateService.Execute(ts => ts.TemplateNamesById(Application.SelectedCardSet.Id)) ?? _templateById;
        StateHasChanged();
    }

    public async Task OnTemplateSelected(int templateId)
    {
        _selectedTemplate = templateId;
        var template = TemplateService.Execute(ts => ts.GetTemplate(templateId));
        if (template == null) return;
        var tags = template.SerializedData().GetTags();
        var modelList = new List<ImportSheetModel>() { new() { CardName = template.Name, TagTexts = tags.ToDictionary(t => t.Tag, t => (object)t.Text) } };
        await _sheetView.UpdateGrid(new DisplaySheetModel<ImportSheetModel>(modelList, minimumRows: 1000,
            dynamicColumns: new() { { "Tags", tags.Select(t => t.Tag).ToList() } }));
    }

    public async Task ImportData()
    {
        if (_selectedTemplate == null) return;
        var template = TemplateService.Execute(ts => ts.GetTemplate(_selectedTemplate.Value));
        if (template == null) return;
        foreach (var newData in (await _sheetView.GetSheetData<ImportSheetModel>() ?? []).Skip(1))
        {
            var tagDictionary = newData.TagTexts.ToDictionary(tt => tt.Key, tt => tt.Value?.ToString() ?? "") ?? [];
            var newCard = template.SerializedData().UpdateTags(tagDictionary);
            CardService.Execute(cs => cs.UpdateCard(new Shared.DTO.CardModel()
            {
                CardSetFk = template.CardSetFk,
                Data = newCard.ToJsonString(),
                Name = newData.CardName,
            }));
        }
    }

    public void Dispose()
    {
        _sheetView?.Dispose();
    }
}
