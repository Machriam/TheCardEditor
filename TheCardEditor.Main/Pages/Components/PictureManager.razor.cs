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

        public void OnFolderSelected(FileDialogResult result)
        {
            PictureService.Execute(ps => ps.LoadPicturesFromPath(result.FilePath));
        }

        protected override void OnInitialized()
        {
            _pictures = PictureService.Execute(ps => ps.GetPictures()).ToList();
            base.OnInitialized();
        }
    }
}
