using Microsoft.AspNetCore.Components;
using TheCardEditor.Main.UiComponents;
using TheCardEditor.Services;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Main.Features.PictureManager
{
    public partial class PictureManager
    {
        [Inject] public ServiceAccessor<PictureService> PictureService { get; set; } = default!;
        private List<PictureModel> _pictures = new();
        private List<string> _relativePaths = new();
        private string _selectedPath = "";
        private Dictionary<long, bool> _existingPictures = new();
        private long? _selectedPicture;

        public void DeletePicture()
        {
            if (_selectedPicture == null) return;
            PictureService.Execute(ps => ps.DeletePicture(_selectedPicture.Value));
            Initialize();
        }

        public string GetBackgroundColor(long id)
        {
            if (_selectedPicture == id) return "lightblue";
            return _existingPictures.TryGetValue(id, out var exists) && exists ? "white" : "yellow";
        }

        public void OnFolderSelected(FileDialogResult result)
        {
            PictureService.Execute(ps => ps.LoadPicturesFromPath(result.FilePath));
            Initialize();
        }

        public void Initialize()
        {
            _selectedPicture = null;
            _pictures = [.. PictureService.Execute(ps => ps.GetPictures())];
            _existingPictures = PictureService.Execute(ps => ps.ValidatePictures()) ?? new();
            _relativePaths = _pictures.Select(p => Path.GetDirectoryName(p.Path) ?? "")
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Distinct()
                        .OrderBy(p => p)
                        .ToList() ?? new();
            StateHasChanged();
        }

        public void PictureSelected(long id)
        {
            _selectedPicture = id;
        }

        public void UpdatePicturePath(FileDialogResult result)
        {
            if (_selectedPicture == null) return;
            PictureService.Execute(ps => ps.UpdatePath(_selectedPicture.Value, result.FilePath));
            Initialize();
        }

        protected override void OnInitialized()
        {
            Initialize();
            base.OnInitialized();
        }
    }
}
