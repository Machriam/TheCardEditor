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

        public async Task OnCoordinatesChanged(int? x, int? y)
        {
            AddObjectX = x ?? AddObjectX;
            AddObjectY = y ?? AddObjectY;
            await _canvasInterop.SetCoordinates(AddObjectX, AddObjectY);
        }

        protected override void OnInitialized()
        {
            ShortcutRegistrator.AddHotKey(ModCode.Ctrl, Code.B, () => ApplyFont(CanvasFontStyle.FontWeight, "bold"), "Bold");
            _currentCard = CardService.Execute(cs => cs.GetCard(CardId));
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
            await _canvasInterop.ImportJson(jsonObject ?? new JsonObject());
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
            _currentCard = CardService.Execute(cs => cs.GetCard(CardId));
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(_currentCard.Data);
            await _canvasInterop.ImportJson(jsonObject ?? new JsonObject());
        }

        public async Task CenterObjects()
        {
            await _canvasInterop.CenterObjects();
        }

        public async Task RemoveObject()
        {
            await _canvasInterop.RemoveObject();
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
            if (json.GetTags().Contains(AddTag))
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
        }

        public async Task SaveCard()
        {
            if (ApplicationStorage.SelectedCardSet == null)
                return;
            var json = await _canvasInterop.ExportJson();
            _currentCard.Data = JsonSerializer.Serialize(json);
            _currentCard.CardSetFk = ApplicationStorage.SelectedCardSet.Id;
            CardService.Execute((s, c) => s.UpdateCard(c), _currentCard);
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
