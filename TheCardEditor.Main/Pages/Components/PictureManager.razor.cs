using Microsoft.AspNetCore.Components;
using TheCardEditor.DataModel.DTO;
using TheCardEditor.Main.UiComponents;
using TheCardEditor.Services;

namespace TheCardEditor.Main.Pages.Components
{
    public partial class PictureManager
    {
        [Inject] public ServiceAccessor<PictureService> PictureService { get; set; } = default!;
        private List<PictureModel> _pictures = new();
        private List<string> _relativePaths = new();
        private string _selectedPath = "";

        public void OnFolderSelected(FileDialogResult result)
        {
            PictureService.Execute(ps => ps.LoadPicturesFromPath(result.FilePath));
            Initialize();

        }
        public void Initialize()
        {
            _pictures = PictureService.Execute(ps => ps.GetPictures()).ToList();
            _relativePaths = _pictures.Select(p => Path.GetDirectoryName(p.Path) ?? "")
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Distinct()
                        .OrderBy(p => p)
                        .ToList() ?? new();
        }

        protected override void OnInitialized()
        {
            Initialize();
            base.OnInitialized();
        }
    }
}
