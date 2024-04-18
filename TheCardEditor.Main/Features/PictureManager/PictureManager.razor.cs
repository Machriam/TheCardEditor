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
    private long? _selectedPicture;

    public void Initialize()
    {
        _selectedPicture = null;
        _pictures = [.. PictureService.Execute(ps => ps.GetPictures())];
        _existingPictures = PictureService.Execute(ps => ps.ValidatePictures()) ?? [];
        StateHasChanged();
    }

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _gridView = GridViewFactory.CreateGrid(this, GridViewId, OnRowsSelected);
        var models = _pictures.Select(p => new PictureGridModel(p.Id)
        {
            PictureName = p.Name,
            Path = p.Path,
            Exists = _existingPictures[p.Id] ? "true" : "false",
            RowColorClass = _existingPictures[p.Id] ? AgGridResources.NoColorRow : AgGridResources.KhakiGridRow
        }).OrderBy(m => m.Exists)
        .ThenBy(m => m.Path);
        await _gridView.UpdateGrid(new DisplayGridModel<PictureGridModel>(models));
    }

    private void OnRowsSelected(long[] id)
    {
    }

    protected override void OnInitialized()
    {
        Initialize();
        base.OnInitialized();
    }
}
