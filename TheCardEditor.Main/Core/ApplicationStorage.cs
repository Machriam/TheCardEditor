using TheCardEditor.DataModel.DTO;

namespace TheCardEditor.Main.Core;

public class ApplicationStorage
{
    public GameModel? SelectedGame { get; set; }
    public CardSetModel? SelectedCardSet { get; set; }

}
