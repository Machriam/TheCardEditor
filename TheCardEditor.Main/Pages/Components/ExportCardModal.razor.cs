using System.Text;
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
            await _folderPicker.SelectFile();
        }

        public async Task FolderPickingFinished(FileDialogResult result)
        {
            var errors = new StringBuilder();
            foreach (var card in CardIds)
            {
                var currentCard = CardService.Execute(cs => cs.GetCard(card));
                if (currentCard == null)
                {
                    errors.Append("Card with ID: ").Append(card).AppendLine(" was not found");
                    continue;
                }
                var pictures = new Dictionary<long, string>();
                var data = currentCard.SerializedData();
                foreach (var picture in data.GetPictureIds())
                {
                    if (pictures.ContainsKey(picture)) continue;
                    var newPicture = PictureService.Execute(ps => ps.GetBase64Picture(picture)) ?? "";
                    if (newPicture.IsEmpty()) errors.Append("Picture: ").Append(picture).AppendLine(" not found");
                    pictures.Add(picture, newPicture);
                }
                await _canvasInterop.ImportJson(data, pictures);
                var png = await _canvasInterop.ExportPng();
                var pngData = new PngExportData() { Name = currentCard.Name, PNG = png };
                File.WriteAllBytes(result.FilePath + "/" + pngData.Name + ".png", pngData.PNGData);
            }
            if (errors.Length != 0) await JsInterop.Prompt(errors.ToString());
            await ModalInstance.CloseAsync();
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
