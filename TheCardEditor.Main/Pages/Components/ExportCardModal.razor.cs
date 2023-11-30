using System.Text.Json;
using System.Text.Json.Nodes;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.UiComponents;
using TheCardEditor.Services;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Pages.Components
{
    public partial class ExportCardModal
    {
        private class PngExportData
        {
            public string Name { get; set; } = "";
            public string PNG { get; set; } = "";
            public byte[] PNGData => Convert.FromBase64String(PNG.Split("base64,").Last());
        }

        private WindowsFolderPicker _folderPicker = default!;

        [CascadingParameter]
        private BlazoredModalInstance ModalInstance { get; set; } = default!;

        [Inject] private ServiceAccessor<CardService> CardService { get; set; } = default!;

        [Inject] private ServiceAccessor<PictureService> PictureService { get; set; } = default!;

        [Inject] private ICanvasInteropFactory CanvasInteropFactory { get; set; } = default!;
        [Inject] private IJsInterop JsInterop { get; set; } = default!;

        [Inject] private ApplicationStorage ApplicationStorage { get; set; } = default!;

        [Parameter] public IEnumerable<long> CardIds { get; set; } = [];

        private int Height { get; set; }

        private int Width { get; set; }
        private ICanvasInterop _canvasInterop { get; set; } = default!;
        private const string ExportCanvasId = nameof(ExportCanvasId);
        private readonly Dictionary<long, string> _existingPictures = new();
        private List<PngExportData> _pngs = [];

        protected override void OnInitialized()
        {
            if (ApplicationStorage.SelectedCardSet == null) return;
            Height = ApplicationStorage.SelectedCardSet.Height;
            Width = ApplicationStorage.SelectedCardSet.Width;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _canvasInterop = CanvasInteropFactory.CreateCanvas(this, ExportCanvasId, OnObjectSelected, OnObjectDeselected, OnMultiObjectIsSelected);
            var errors = "";
            foreach (var card in CardIds)
            {
                var currentCard = CardService.Execute(cs => cs.GetCard(card));
                if (currentCard == null)
                {
                    errors += "Card with ID: " + card + " was not found\n";
                    continue;
                }
                foreach (var picture in currentCard.SerializedData().GetPictureIds())
                {
                    if (!_existingPictures.ContainsKey(picture))
                    {
                        var newPicture = PictureService.Execute(ps => ps.GetBase64Picture(picture)) ?? "";
                        if (newPicture.IsEmpty()) errors += "Picture: " + picture + " not found";
                        _existingPictures.Add(picture, newPicture);
                    }
                }
                var jsonObject = JsonSerializer.Deserialize<JsonObject>(currentCard.Data);
                await _canvasInterop.ImportJson(jsonObject ?? [], _existingPictures);
                var png = await _canvasInterop.ExportPng();
                await JsInterop.ConsoleLog(png);
                _pngs.Add(new PngExportData()
                {
                    PNG = png,
                    Name = currentCard.Name,
                });
            }
            if (!string.IsNullOrEmpty(errors)) await JsInterop.Prompt(errors);
            await _folderPicker.SelectFile();
            await ModalInstance.CloseAsync();
        }

        public Task FolderPickingFinished(FileDialogResult result)
        {
            foreach (var png in _pngs)
            {
                File.WriteAllBytes(result.FilePath + "/" + png.Name + ".png", png.PNGData);
            }
            return Task.CompletedTask;
        }

        [JSInvokable]
        public void OnObjectDeselected()
        {
        }

        [JSInvokable]
        public void OnMultiObjectIsSelected()
        {
        }

        [JSInvokable]
        public void OnObjectSelected(ObjectParameter param)
        {
        }
    }
}
