using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TheCardEditor.DataModel.DTO;
using TheCardEditor.Main.Core;
using TheCardEditor.Services;

namespace TheCardEditor.Main.Pages.Components
{
    public partial class Home
    {
        [Inject]
        private ServiceAccessor<FontService> FontService { get; set; } = default!;

        [Inject]
        private ServiceAccessor<GameService> GameService { get; set; } = default!;

        [Inject]
        private ServiceAccessor<CardSetService> CardSetService { get; set; } = default!;

        [Inject]
        private IJsInterop JsInterop { get; set; } = default!;

        [Inject]
        private ApplicationStorage ApplicationStorage { get; set; } = default!;

        private FontModel _selectedFont = new();
        private IEnumerable<FontModel> Fonts { get; set; } = new List<FontModel>();
        private IEnumerable<GameModel> Games { get; set; } = new List<GameModel>();
        private IEnumerable<CardSetModel> CardSets { get; set; } = new List<CardSetModel>();
        private GameModel _selectedGame = new();
        private CardSetModel _selectedCardSet = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;
            Fonts = FontService.Execute(f => f.GetFonts());
            Games = GameService.Execute(s => s.GetGames());
            foreach (var font in Fonts)
            {
                await JsInterop.LoadFont(font.Name, font.Base64Data);
            }

            StateHasChanged();
        }

        public async Task SelectCardAndGame()
        {
            ApplicationStorage.SelectedCardSet = _selectedCardSet;
            ApplicationStorage.SelectedGame = _selectedGame;
            await ApplicationStorage.OnCardSelectionChanged();
        }

        public void GameSelected(GameModel model)
        {
            _selectedGame = model;
            ReloadCardSets();
        }

        private void ReloadCardSets()
        {
            CardSets = CardSetService.Execute((s, g) => s.GetCardSets(g.Id), _selectedGame);
            _selectedCardSet = new()
            {
                GameFk = _selectedGame.Id,
                Name = ""
            };
            StateHasChanged();
        }

        private void ReloadGames()
        {
            Games = GameService.Execute(s => s.GetGames());
            _selectedGame = new();
            StateHasChanged();
        }

        public void DeleteCardSet()
        {
            CardSetService.Execute((s, c) => s.DeleteCardSet(c), _selectedCardSet);
            ReloadCardSets();
        }

        public void UpsertCardSet()
        {
            CardSetService.Execute((s, c) => s.UpdateCardSet(c), _selectedCardSet);
            ReloadCardSets();
        }

        public void UpsertGame()
        {
            GameService.Execute((s, g) => s.UpdateGame(g), _selectedGame);
            ReloadGames();
        }

        public void DeleteGame()
        {
            GameService.Execute((s, g) => s.DeleteGame(g), _selectedGame);
            ReloadGames();
        }

        public async Task LoadFile(InputFileChangeEventArgs args)
        {
            var memoryStream = new MemoryStream();
            await args.File.OpenReadStream(1024 * 1014 * 4).CopyToAsync(memoryStream);
            var base64File = Convert.ToBase64String(memoryStream.ToArray());
            _selectedFont.Base64Data = base64File;
        }

        public void DeleteFont()
        {
            FontService.Execute((s, f) => s.DeleteFont(f), _selectedFont);
            ReloadFonts();
        }

        public async Task UpsertFont()
        {
            FontService.Execute((s, f) => s.UpdateFont(f), _selectedFont);
            await JsInterop.LoadFont(_selectedFont.Name, _selectedFont.Base64Data);
            ReloadFonts();
        }

        private void ReloadFonts()
        {
            Fonts = FontService.Execute(f => f.GetFonts());
            _selectedFont = new();
            StateHasChanged();
        }
    }
}
