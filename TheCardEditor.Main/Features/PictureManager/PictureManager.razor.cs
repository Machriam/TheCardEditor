using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.Core.Grid;
using TheCardEditor.Main.UiComponents;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Main.Features.PictureManager;

public class PictureGridModel(long id) : AbstractGridModel(id)
{
    [GridMetaData(HeaderName = "Name", Width = 400, Resizable = true)]
    public string PictureName { get; set; } = "";

    [GridMetaData(HeaderName = "Path", Width = 1200, Resizable = true)]
    public string Path { get; set; } = "";

    [GridMetaData(HeaderName = "Found")]
    public string Exists { get; set; } = "";
}

public partial class PictureManager
{
    [Inject] public ServiceAccessor<PictureService> PictureService { get; set; } = default!;
    [Inject] private IGridViewFactory GridViewFactory { get; set; } = default!;
    private IGridView _gridView = default!;
    private const string GridViewId = nameof(PictureManager) + nameof(GridViewId);
    private List<PictureModel> _pictures = [];
    private string _selectedPath = "";
    private Dictionary<long, bool> _existingPictures = [];
    private long[] _selectedPictures = [];

    public async Task Initialize()
    {
        _pictures = [.. PictureService.Execute(ps => ps.GetPictures())];
        _existingPictures = PictureService.Execute(ps => ps.ValidatePictures()) ?? [];
        var models = _pictures.Select(p => new PictureGridModel(p.Id)
        {
            PictureName = p.Name,
            Path = p.Path,
            Exists = _existingPictures[p.Id] ? "true" : "false",
            RowColorClass = _existingPictures[p.Id] ? AgGridResources.NoColorRow : AgGridResources.KhakiGridRow
        }).OrderBy(m => m.Exists).ThenBy(m => m.Path);
        await _gridView.UpdateGrid(new DisplayGridModel<PictureGridModel>(models));
    }

    public async Task DeletePictures()
    {
        if (_selectedPictures.Length == 0) return;
        PictureService.Execute(ps => ps.DeletePictures(_selectedPictures));
        await Initialize();
    }

    public async Task OnFolderSelected(FileDialogResult result)
    {
        PictureService.Execute(ps => ps.LoadPicturesFromPath(result.FilePath));
        await Initialize();
    }

    public async Task UpdatePicturePath(FileDialogResult result)
    {
        if (_selectedPictures.Length != 1) return;
        PictureService.Execute(ps => ps.UpdatePath(_selectedPictures[0], result.FilePath));
        await Initialize();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _gridView = GridViewFactory.CreateGrid(this, GridViewId, OnRowsSelected);
        await Initialize();
    }

    [JSInvokable]
    public void OnRowsSelected(long[] ids)
    {
        _selectedPictures = ids;
    }
}
