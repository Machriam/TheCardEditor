using TheCardEditor.DataModel.DTO;

namespace TheCardEditor.Main.Core;

public class ApplicationStorage
{
    public event Func<Task>? CardSelectionChanged;

    public GameModel? SelectedGame { get; set; }
    public CardSetModel? SelectedCardSet { get; set; }
    public List<string> AvailableFonts { get; set; } = new();

    public async Task OnCardSelectionChanged()
    {
        await (CardSelectionChanged?.Invoke() ?? Task.CompletedTask);
    }
}
