using System.Text.Json;
using System.Text.Json.Nodes;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.DataModel.DTO;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Pages.Components
{
    public partial class CardModal
    {
        [CascadingParameter]
        private BlazoredModalInstance ModalInstance { get; set; } = default!;

        [Inject]
        private ServiceAccessor<CardService> CardService { get; set; } = default!;

        [Inject]
        private ServiceAccessor<PictureService> PictureService { get; set; } = default!;

        [Inject]
        private ICanvasInteropFactory CanvasInteropFactory { get; set; } = default!;

        [Inject]
        private IJsInterop JsInterop { get; set; } = default!;

        [Inject]
        private IShortcutRegistrator ShortcutRegistrator { get; set; } = default!;

        [Inject]
        private ApplicationStorage ApplicationStorage { get; set; } = default!;

        [Parameter]
        public long? CardId { get; set; }

        [Parameter]
        public List<string> Tags { get; set; } = new();

        private int Height { get; set; }

        private int Width { get; set; }

        private CardModel _currentCard = new();
        private string AddNewText { get; set; } = "";
        private string AddTag { get; set; } = "";
        private int AddObjectX { get; set; }

        private int AddObjectY { get; set; }
        private bool _multipleObjectsAreSelected;

        private string _selectedFont = "";
        private int FontSize { get; set; } = 12;
        private ICanvasInterop _canvasInterop = default!;
        private const string CanvasId = "CardCanvasId";
        private Dictionary<long, string> _pictureData = new();
        private Dictionary<long, PictureModel> _pictureById = new();
        private PictureModel? _selectedPicture;
        private int _selectedIndex;

        public async Task OnCoordinatesChanged(int? x, int? y)
        {
            AddObjectX = x ?? AddObjectX;
            AddObjectY = y ?? AddObjectY;
            await _canvasInterop.SetCoordinates(AddObjectX, AddObjectY);
        }

        public void SelectedPictureChanged(IDataListItem picture)
        {
            _selectedPicture = picture.GetItem<PictureModel>();
        }

        private async Task SelectObject()
        {
            await _canvasInterop.SelectObject(_selectedIndex);
        }

        protected override void OnInitialized()
        {
            ShortcutRegistrator.AddHotKey(ModCode.Ctrl, Code.B, () => ApplyFont(CanvasFontStyle.FontWeight, "bold"), "Bold");
            _currentCard = CardService.Execute(cs => cs.GetCard(CardId)) ?? new();
            _pictureData = _currentCard.SerializedData().GetPictureIds()
                .Distinct()
                .ToDictionary(id => id, id => PictureService.Execute(ps => ps.GetBase64Picture(id)) ?? "");
            _pictureById = PictureService.Execute(ps => ps.GetPictures()).ToDictionary(p => p.Id);
            if (ApplicationStorage.SelectedCardSet == null) return;
            _selectedFont = ApplicationStorage.AvailableFonts.FirstOrDefault() ?? "Arial";
            Height = (int)ApplicationStorage.SelectedCardSet.Height;
            Width = (int)ApplicationStorage.SelectedCardSet.Width;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _canvasInterop = CanvasInteropFactory.CreateCanvas(this, CanvasId, OnObjectSelected, OnObjectDeselected, OnMultiObjectIsSelected);
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(_currentCard.Data);
            await _canvasInterop.ImportJson(jsonObject ?? new JsonObject(), _pictureData);
            StateHasChanged();
        }

        [JSInvokable]
        public void OnObjectDeselected()
        {
            _multipleObjectsAreSelected = false;
            AddObjectX = 0;
            AddObjectY = 0;
            StateHasChanged();
        }

        [JSInvokable]
        public void OnMultiObjectIsSelected()
        {
            _multipleObjectsAreSelected = true;
            StateHasChanged();
        }

        [JSInvokable]
        public void OnObjectSelected(float left, float top, string tag)
        {
            _multipleObjectsAreSelected = false;
            AddObjectX = (int)left;
            AddObjectY = (int)top;
            AddTag = tag;
            StateHasChanged();
        }

        private async Task Reset()
        {
            _currentCard = CardService.Execute(cs => cs.GetCard(CardId)) ?? new();
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(_currentCard.Data);
            await _canvasInterop.ImportJson(jsonObject ?? new JsonObject(), _pictureData);
        }

        public async Task CenterObjects()
        {
            await _canvasInterop.CenterObjects();
        }

        public async Task RemoveObject()
        {
            await _canvasInterop.RemoveObject();
            await UpdateVirtualData();
        }

        public async Task InsertPicture()
        {
            if (_selectedPicture == null) return;
            var base64Text = PictureService.Execute(ps => ps.GetBase64Picture(_selectedPicture.Id)) ?? "";
            await _canvasInterop.DrawPicture(AddObjectX, AddObjectY, _selectedPicture.Id, base64Text);
            await UpdateVirtualData();
        }

        public async Task AskForNewTag()
        {
            var newTag = await JsInterop.Prompt("Name of new Tag?");
            if (string.IsNullOrWhiteSpace(newTag)) return;
            if (Tags.Contains(newTag))
            {
                await JsInterop.LogError("Tag exists already");
                return;
            }
            Tags.Add(newTag);
            AddTag = newTag;
            StateHasChanged();
        }

        public async Task AddText()
        {
            var json = await _canvasInterop.ExportJson();
            if (json.GetTags().Select(t => t.Tag).Contains(AddTag))
            {
                await JsInterop.LogError("Tag already exists");
                return;
            }
            if (string.IsNullOrWhiteSpace(AddTag))
            {
                await JsInterop.LogError("Each textbox must have a tag");
                return;
            }
            await _canvasInterop.DrawText(AddObjectX, AddObjectY, AddNewText, AddTag);
            await UpdateVirtualData();
        }

        public async Task UpdateVirtualData()
        {
            if (ApplicationStorage.SelectedCardSet == null) return;
            var json = await _canvasInterop.ExportJson();
            _selectedIndex = -1;
            _currentCard.VirtualData = JsonSerializer.Serialize(json);
            StateHasChanged();
        }

        public async Task SaveCard()
        {
            if (ApplicationStorage.SelectedCardSet == null) return;
            var json = await _canvasInterop.ExportJson();
            _currentCard.Data = JsonSerializer.Serialize(json);
            _currentCard.CardSetFk = ApplicationStorage.SelectedCardSet.Id;
            CardService.Execute((s, c) => s.UpdateCard(c), _currentCard);
            _pictureData = _currentCard.SerializedData().GetPictureIds()
                            .Distinct()
                            .ToDictionary(id => id, id => PictureService.Execute(ps => ps.GetBase64Picture(id)) ?? "");
        }

        private async ValueTask ApplyFont(CanvasFontStyle style, object value)
        {
            if (style == CanvasFontStyle.Fill)
            {
                value = await JsInterop.Prompt("Enter colorcode:");
            }
            await _canvasInterop.ApplyFont(style, value);
        }

        public void Dispose()
        {
            ShortcutRegistrator.Dispose();
        }

        public async Task Close()
        {
            Dispose();
            await ModalInstance.CloseAsync();
        }
    }
}
