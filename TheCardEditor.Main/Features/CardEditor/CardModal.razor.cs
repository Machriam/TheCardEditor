using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using TheCardEditor.Shared.DTO;
using TheCardEditor.Shared.Features.CardEditor;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Features.CardEditor
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
        private ServiceAccessor<TemplateService> TemplateService { get; set; } = default!;

        [Inject]
        private ICanvasInteropFactory CanvasInteropFactory { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private IShortcutRegistrator ShortcutRegistrator { get; set; } = default!;

        [Inject]
        private ApplicationStorage ApplicationStorage { get; set; } = default!;

        [Parameter]
        public long? CardId { get; set; }

        [Parameter]
        public List<string> Tags { get; set; } = new();

        [Parameter]
        public string? Template { get; set; }

        private int Height { get; set; }

        private int Width { get; set; }

        private CardModel _currentCard = new();
        private const string AddNewText = "New Textbox";
        private string AddTag { get; set; } = "";
        private int AddObjectX { get; set; }

        private int AddObjectY { get; set; }
        private decimal AddObjectAngle { get; set; }
        private bool _multipleObjectsAreSelected;

        private string _selectedFont = "";
        private int FontSize { get; set; } = 32;
        private ICanvasInterop _canvasInterop = default!;
        private const string CanvasId = "CardCanvasId";
        private Dictionary<long, string> _pictureData = new();
        private Dictionary<long, PictureModel> _pictureById = new();
        private PictureModel? _selectedPicture;
        private ObjectParameter? _selectedObjectParams;
        private int _selectedIndex;

        public async Task OnCoordinatesChanged(int? x, int? y, decimal? angle)
        {
            AddObjectX = x ?? AddObjectX;
            AddObjectY = y ?? AddObjectY;
            AddObjectAngle = angle ?? AddObjectAngle;
            await _canvasInterop.SetCoordinates(AddObjectX, AddObjectY, AddObjectAngle);
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
            if (!string.IsNullOrEmpty(Template)) _currentCard.Data = Template;
            _pictureData = _currentCard.SerializedData().GetPictureIds()
                .Distinct()
                .ToDictionary(id => id, id => PictureService.Execute(ps => ps.GetBase64Picture(id)) ?? "");
            _pictureById = PictureService.Execute(ps => ps.GetPictures()).ToDictionary(p => p.Id);
            if (ApplicationStorage.SelectedCardSet == null) return;
            _selectedFont = ApplicationStorage.AvailableFonts.FirstOrDefault() ?? "Arial";
            Height = (int)(ApplicationStorage.SelectedCardSet.Height * ApplicationStorage.SelectedCardSet.Zoom / 100f);
            Width = (int)(ApplicationStorage.SelectedCardSet.Width * ApplicationStorage.SelectedCardSet.Zoom / 100f);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _canvasInterop = CanvasInteropFactory.CreateCanvas(this, CanvasId, OnObjectSelected,
                OnObjectDeselected, OnMultiObjectIsSelected);
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(_currentCard.Data);
            await _canvasInterop.ImportJson(jsonObject ?? [], _pictureData);
            await _canvasInterop.Zoom(ApplicationStorage.SelectedCardSet?.Zoom ?? 100d);
            StateHasChanged();
        }

        [JSInvokable]
        public Task OnObjectDeselected()
        {
            _multipleObjectsAreSelected = false;
            AddObjectX = 0;
            AddObjectY = 0;
            AddObjectAngle = 0;
            StateHasChanged();
            return Task.CompletedTask;
        }

        [JSInvokable]
        public Task OnMultiObjectIsSelected()
        {
            _multipleObjectsAreSelected = true;
            StateHasChanged();
            return Task.CompletedTask;
        }

        [JSInvokable]
        public async Task OnObjectSelected(ObjectParameter param)
        {
            _multipleObjectsAreSelected = false;
            _selectedObjectParams = param;
            AddObjectX = (int)param.Left;
            AddObjectY = (int)param.Top;
            AddObjectAngle = (decimal)param.Angle;
            FontSize = param.TextSize ?? FontSize;
            AddTag = param.Tag ?? "";
            StateHasChanged();
        }

        private async Task AddFilter()
        {
            if (_selectedObjectParams == null || _multipleObjectsAreSelected) return;
            var base64Text = PictureService.Execute(ps => ps.GetBase64Picture(_selectedObjectParams.Value.PictureId)) ?? "";
            var pipeline = new ImageFilterPipeline()
            {
                Filters = [
                    new ImageFilterModel() { Name = "TransparentFilter", Parameters = [
                    new FilterParameter() { Name="Threshold 1",Type=FilterParameterType.Double,Value="100" },
                    new FilterParameter() { Name="Threshold 2",Type=FilterParameterType.Double,Value="300" },
                    new FilterParameter() { Name="Aperture Size",Type=FilterParameterType.Int,Value="3" },
                    new FilterParameter() { Name="L2 Gradient",Type=FilterParameterType.Bool,Value="false" },
                ] } ]
            };
            var cannyImage = await JS.ExecuteModuleFunction<string>("ApplyFilterPipeline",
                [base64Text, pipeline], "/lib/OpenCvInterop.js");
            await _canvasInterop.UpdateImage(cannyImage, pipeline);
            await UpdateVirtualData();
        }

        private async Task Reset()
        {
            _currentCard = CardService.Execute(cs => cs.GetCard(CardId)) ?? new();
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(_currentCard.Data);
            await _canvasInterop.ImportJson(jsonObject ?? [], _pictureData);
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

        public async Task SaveAsTemplate()
        {
            await UpdateVirtualData();
            var name = await JS.GetUserString("Enter name for new Template");
            TemplateService.Execute(ts => ts.StoreTemplate(name, _currentCard));
        }

        public async Task InsertPicture()
        {
            if (_selectedPicture == null) return;
            var base64Text = PictureService.Execute(ps => ps.GetBase64Picture(_selectedPicture.Id)) ?? "";
            await _canvasInterop.DrawPicture(AddObjectX, AddObjectY, _selectedPicture.Id, _selectedPicture.Name, base64Text);
            await UpdateVirtualData();
        }

        public async Task AskForNewTag()
        {
            var newTag = await JS.GetUserString("Name of new Tag?");
            if (string.IsNullOrWhiteSpace(newTag)) return;
            if (Tags.Contains(newTag))
            {
                await JS.LogError("Tag exists already");
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
                await JS.LogError("Tag already exists");
                return;
            }
            if (string.IsNullOrWhiteSpace(AddTag))
            {
                await JS.LogError("Each textbox must have a tag");
                return;
            }
            await _canvasInterop.DrawText(AddObjectX, AddObjectY, AddNewText, AddTag, FontSize);
            await UpdateVirtualData();
        }

        public async Task UpdateVirtualData(bool resetSelectedIndex = true)
        {
            if (ApplicationStorage.SelectedCardSet == null) return;
            var json = await _canvasInterop.ExportJson();
            if (resetSelectedIndex) _selectedIndex = -1;
            _currentCard.VirtualData = JsonSerializer.Serialize(json);
            StateHasChanged();
        }

        private async Task BringForward()
        {
            _selectedIndex = await _canvasInterop.BringForward(_selectedIndex);
            await UpdateVirtualData(false);
        }

        private async Task SendBackwards()
        {
            _selectedIndex = await _canvasInterop.SendBackwards(_selectedIndex);
            await UpdateVirtualData(false);
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
                value = await JS.GetUserString("Enter colorcode:");
            }
            await _canvasInterop.ApplyFont(style, value);
        }

        public void Dispose()
        {
            ShortcutRegistrator.Dispose();
        }

        public async Task Close()
        {
            await UpdateVirtualData();
            if (_currentCard.IsModified() && !await JS.Confirm("You have unsaved data. Do you really want to exit?")) return;
            Dispose();
            await ModalInstance.CloseAsync();
        }
    }
}
