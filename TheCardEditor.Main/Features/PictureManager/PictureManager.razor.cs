using System.Diagnostics;
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
    public string Status { get; set; } = "";
}

public partial class PictureManager
{
    [Inject] public ServiceAccessor<PictureService> PictureService { get; set; } = default!;
    [Inject] private IGridViewFactory GridViewFactory { get; set; } = default!;
    private IGridView _gridView = default!;
    private const string GridViewId = nameof(PictureManager) + nameof(GridViewId);
    private List<PictureModel> _pictures = [];
    private Dictionary<long, bool> _existingPictures = [];
    private long[] _selectedPictures = [];
    private Dictionary<long, PictureGridModel> _newPictureByNegativeId = [];

    public async Task ShowNewPictures()
    {
        var knownPictures = _pictures.Select(p => p.Path).ToHashSet();
        var newPictures = _pictures.Select(p => Path.GetDirectoryName(p.Path) ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .SelectMany(p => Directory.GetFiles(p))
            .Where(p => AppSettings.AllowedPictureTypes.Contains(Path.GetExtension(p)) && !knownPictures.Contains(p));
        var newPictureModels = newPictures.WithIndex().Select(p => new PictureGridModel(-(p.Index + 1))
        {
            PictureName = Path.GetFileName(p.Item),
            Path = p.Item,
            Status = "new",
            RowColorClass = AgGridResources.AquamarineGridRow
        });
        await Initialize(newPictureModels.ToArray());
    }

    public async Task AddSelectedNewPictures()
    {
        var newSelectedPictures = _selectedPictures.Where(_newPictureByNegativeId.ContainsKey).ToList();
        if (newSelectedPictures.Count == 0) return;
        PictureService.Execute(ps => ps.AddPicturesByPath(newSelectedPictures.Select(np => _newPictureByNegativeId[np].Path)));
        await Initialize();
    }

    public async Task Initialize(params PictureGridModel[] newCards)
    {
        _pictures = [.. PictureService.Execute(ps => ps.GetPictures())];
        _existingPictures = PictureService.Execute(ps => ps.ValidatePictures()) ?? [];
        var models = _pictures.Select(p => new PictureGridModel(p.Id)
        {
            PictureName = p.Name,
            Path = p.Path,
            Status = _existingPictures.TryGetValue(p.Id, out var exists) ? exists ? "true" : "false" : "new",
            RowColorClass = _existingPictures[p.Id] ? AgGridResources.NoColorRow : AgGridResources.KhakiGridRow
        })
        .AppendRange(newCards)
        .OrderBy(m => m.Status)
        .ThenBy(m => m.Path);
        _newPictureByNegativeId = newCards.ToDictionary(nc => nc.Id);
        await _gridView.UpdateGrid(new DisplayGridModel<PictureGridModel>(models));
    }

    public async Task DeletePictures()
    {
        if (_selectedPictures.Length == 0) return;
        PictureService.Execute(ps =>
            ps.DeletePictures(_selectedPictures.Where(p => _existingPictures.ContainsKey(p)).ToArray()));
        await Initialize();
    }

    public async Task OnFolderSelected(FileDialogResult result)
    {
        PictureService.Execute(ps => ps.LoadPicturesFromPath(result.FilePath));
        await Initialize();
    }

    public async Task UpdatePicturePath(FileDialogResult result)
    {
        if (_selectedPictures.Length != 1 && _selectedPictures[0] <= 0) return;
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
        StateHasChanged();
    }
}
