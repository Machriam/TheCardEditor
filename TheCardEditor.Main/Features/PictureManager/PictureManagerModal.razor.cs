using Microsoft.AspNetCore.Components;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Main.Features.PictureManager;

public partial class PictureManagerModal
{
    [Parameter] public List<CardModel> Cards { get; set; } = [];
    [Parameter] public List<GameSet> Sets { get; set; } = [];
    private Dictionary<int, (string Game, string CardSet)> _gameSetById = [];

    protected override Task OnInitializedAsync()
    {
        _gameSetById = Sets.ToDictionary(s => s.CardSet.Id, s => (Game: s.GameModel.Name, CardSet: s.CardSet.Name));
        return base.OnInitializedAsync();
    }
}
