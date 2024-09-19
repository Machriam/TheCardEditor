using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using TheCardEditor.SheetComponent;

namespace TheCardEditor.Main.Features.ImportManager;

public partial class ImportManager : IDisposable
{
    public record struct SheetRowCard(ImportSheetModel SheetModel, int? CardId, string Color, bool IgnoreInImport, int RowIndex);

    public class ImportSheetModel : AbstractSheetModel
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
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private const string NewLineReplacement = "{br}";

    private static readonly HighlightData[] _rowColors = [new HighlightData("yellow"), new HighlightData("red"), new HighlightData("white")];

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
        var modelList = new List<ImportSheetModel>() { new() { CardName = template.Name,
            TagTexts = tags.ToDictionary(t => t.Tag, t => (object)t.Text.Replace("\n",NewLineReplacement)) } };
        await _sheetView.UpdateGrid(new DisplaySheetModel<ImportSheetModel>(modelList,
            highlightCellsDictionary: _rowColors,
            minimumRows: 1000,
            dynamicColumns: new() { { "Tags", tags.Select(t => t.Tag).ToList() } }));
    }

    public async Task CopyData()
    {
        var sheetData = (await _sheetView.GetSheetData<ImportSheetModel>() ?? []).ToList();
        if (sheetData.Count == 0) return;

        var headers = sheetData[0].TagTexts.Select(tt => tt.Key).ToList();
        var result = new StringBuilder();
        result.Append("Card Name").Append('\t');
        foreach (var header in headers) result.Append(header).Append('\t');
        foreach (var data in sheetData.Skip(1))
        {
            result.Remove(result.Length - 1, 1).AppendLine();
            result.Append(data.CardName).Append('\t');
            foreach (var header in headers)
            {
                var appendText = data.TagTexts.TryGetValue(header, out var value) ? value.ToString() : "";
                result.Append(value).Append('\t');
            }
        }
        result.Remove(result.Length - 1, 1);
        await JS.CopyToClipboard(result.ToString());
    }

    public async Task<IEnumerable<SheetRowCard>> DataToImport()
    {
        var result = new List<SheetRowCard>();
        if (_selectedTemplate == null) return result;
        var template = TemplateService.Execute(ts => ts.GetTemplate(_selectedTemplate.Value));
        if (template == null) return result;
        var templateTags = template.SerializedData().GetTags().Select(t => t.Tag).ToHashSet();
        var existingCardNames = CardService.Execute(cs => cs.CardsOfSet(template.CardSetFk))
            .ToDictionary(c => c.Name, c => c.Id);
        foreach (var newData in (await _sheetView.GetSheetData<ImportSheetModel>() ?? []).Skip(1).WithIndex())
        {
            var existingCard = existingCardNames.TryGetValue(newData.Item.CardName, out var cardRef) ?
                CardService.Execute(cs => cs.GetCard(cardRef)) : null;
            var tagsOfCard = existingCard?.SerializedData().GetTags()
                .Select(t => t.Tag).ToHashSet() ?? [];
            tagsOfCard.SymmetricExceptWith(templateTags);
            if (existingCard != null && tagsOfCard.Count != 0)
            {
                result.Add(new(newData.Item, null, "red", true, newData.Index + 2));
                continue;
            }
            if (existingCardNames.TryGetValue(newData.Item.CardName, out var cardId))
            {
                result.Add(new(newData.Item, cardId, "yellow", false, newData.Index + 2));
                continue;
            }
            if (!existingCardNames.ContainsKey(newData.Item.CardName))
                existingCardNames.Add(newData.Item.CardName, 0);
            result.Add(new(newData.Item, null, "white", false, newData.Index + 2));
        }
        await _sheetView.HighlightRows(result.ToDictionary(r => r.RowIndex, r => r.Color));
        return result;
    }

    public async Task ImportData()
    {
        if (_selectedTemplate == null) return;
        var template = TemplateService.Execute(ts => ts.GetTemplate(_selectedTemplate.Value));
        if (template == null) return;
        var importData = await DataToImport();
        foreach (var newData in importData.Where(d => !d.IgnoreInImport))
        {
            var tagDictionary = newData.SheetModel.TagTexts
                .ToDictionary(tt => tt.Key, tt => tt.Value?.ToString()?.Replace(NewLineReplacement, "\n") ?? "") ?? [];
            var newCard = template.SerializedData().UpdateTags(tagDictionary);
            CardService.Execute(cs => cs.UpdateCard(new Shared.DTO.CardModel()
            {
                CardSetFk = template.CardSetFk,
                Id = newData.CardId ?? 0,
                Data = newCard.ToJsonString(),
                Name = newData.SheetModel.CardName,
            }));
        }
    }

    public void Dispose()
    {
        _sheetView?.Dispose();
    }
}
