using System.Diagnostics.CodeAnalysis;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.DataModel.DTO;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;
using TheCardEditor.Shared;

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
        private IReadOnlyDictionary<long, string> _templateNamesById = new Dictionary<long, string>();
        private long? _selectedTemplate;

        protected override void OnInitialized()
        {
            if (ApplicationStorage.SelectedCardSet == null) return;
            _templateNamesById = TemplateService.Execute(ts => ts.TemplateNamesById(ApplicationStorage.SelectedCardSet.Id)) ?? new Dictionary<long, string>();
            Height = (int)ApplicationStorage.SelectedCardSet.Height;
            Width = (int)ApplicationStorage.SelectedCardSet.Width;
        }

        public async Task RenderCanvas(TemplateModel template)
        {
            _canvasInterop?.Dispose();
            _canvasInterop = CanvasInteropFactory.CreateCanvas(this, CanvasId, OnObjectSelected, OnObjectDeselected, OnMultiObjectIsSelected);
            var pictureData = template.SerializedData().GetPictureIds()
                .Distinct()
                .ToDictionary(id => id, id => PictureService.Execute(ps => ps.GetBase64Picture(id)) ?? "");
            await _canvasInterop.ImportJson(template.SerializedData(), pictureData);
        }

        public async Task DeleteTemplate()
        {
            if (_selectedTemplate == null) return;
            var confirmation = await JsInterop.Confirm("Do you really want to delete the template?");
            if (!confirmation) return;
            TemplateService.Execute(ts => ts.DeleteTemplate(_selectedTemplate.Value));
            if (ApplicationStorage.SelectedCardSet == null) return;
            _selectedTemplate = null;
            _templateNamesById = TemplateService.Execute(ts => ts.TemplateNamesById(ApplicationStorage.SelectedCardSet.Id)) ?? new Dictionary<long, string>();
            await _canvasInterop.Reset();
            StateHasChanged();
        }

        public async Task UseTemplateForNewCard()
        {
            if (_selectedTemplate == null) { await Close(); return; }
            var template = TemplateService.Execute(ts => ts.GetTemplate(_selectedTemplate.Value));
            if (template == null) { await Close(); return; }
            await ModalInstance.CloseAsync(ModalResult.Ok(template.Data));
        }

        public async Task TemplateSelected(long id)
        {
            _selectedTemplate = id;
            var template = TemplateService.Execute(ts => ts.GetTemplate(id));
            if (template == null) return;
            await RenderCanvas(template);
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
