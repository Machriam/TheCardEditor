using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.DataModel.DTO;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;

namespace TheCardEditor.Main.Pages.Components
{
    public partial class TemplateModal
    {
        [CascadingParameter]
        private BlazoredModalInstance ModalInstance { get; set; } = default!;

        [Inject]
        private ServiceAccessor<PictureService> PictureService { get; set; } = default!;

        [Inject]
        private ServiceAccessor<TemplateService> TemplateService { get; set; } = default!;

        [Inject]
        private ICanvasInteropFactory CanvasInteropFactory { get; set; } = default!;

        [Inject]
        private IJsInterop JsInterop { get; set; } = default!;

        [Inject]
        private ApplicationStorage ApplicationStorage { get; set; } = default!;

        private int Height { get; set; }

        private int Width { get; set; }

        private ICanvasInterop _canvasInterop = default!;
        private const string CanvasId = "TemplateCanvasId";
        private Dictionary<long, string> _pictureData = new();
        private Dictionary<long, PictureModel> _pictureById = new();

        protected override void OnInitialized()
        {
            _pictureById = PictureService.Execute(ps => ps.GetPictures()).ToDictionary(p => p.Id);
            if (ApplicationStorage.SelectedCardSet == null) return;
            Height = (int)ApplicationStorage.SelectedCardSet.Height;
            Width = (int)ApplicationStorage.SelectedCardSet.Width;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _canvasInterop = CanvasInteropFactory.CreateCanvas(this, CanvasId, OnObjectSelected, OnObjectDeselected, OnMultiObjectIsSelected);
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
        public void OnObjectSelected(float left, float top, string tag)
        {
        }

        public void Dispose()
        {
        }

        public async Task Close()
        {
            await ModalInstance.CloseAsync();
        }
    }
}
