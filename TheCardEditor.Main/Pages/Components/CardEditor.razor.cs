using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.Core.Grid;
using TheCardEditor.Services;

namespace TheCardEditor.Main.Pages.Components
{
    public class CardGridModel : AbstractGridModel
    {
        public CardGridModel(long id) : base(id)
        {
        }

        [GridMetaData(HeaderName = "Name")]
        public string Name { get; set; } = "";

        [GridMetaData(HeaderName = "Data", Resizable = true)]
        public string Data { get; set; } = "";
    }

    public partial class CardEditor : IDisposable
    {
        private const string GridId = "CardGrid";
        [Inject] private IGridViewFactory GridViewFactory { get; set; } = default!;
        [Inject] private ServiceAccessor<CardService> CardService { get; set; } = default!;
        [Inject] private ApplicationStorage ApplicationStorage { get; set; } = default!;
        [Inject] private IModalHelper ModalHelper { get; set; } = default!;
        private long[] _selectedCards = new long[0];
        private Dictionary<long, CardGridModel> _cardById = new();
        private IGridView _gridView = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _gridView = GridViewFactory.CreateGrid(this, GridId, OnCardsSelected);
                if (ApplicationStorage.SelectedCardSet == null) return;
                _cardById = CardService.Execute(cs => cs.GetCards(ApplicationStorage.SelectedCardSet.Id))
                     .Select(c => new CardGridModel(c.Id)
                     {
                         Name = c.Name,
                         Data = c.Data
                     }).ToDictionary(c => c.Id);
                await _gridView.UpdateGrid(new DisplayGridModel<CardGridModel>(_cardById.Values));
            }
            base.OnAfterRender(firstRender);
        }

        [JSInvokable]
        public void OnCardsSelected(long[] ids)
        {
            _selectedCards = ids;
            StateHasChanged();
        }

        public async Task NewCard()
        {
            await ModalHelper.ShowModal<CardModal>("Create new Card", new() {
                { nameof(CardModal.CardId), null }});
        }

        public async Task EditCards()
        {
            if (_selectedCards.Length == 0) return;
            await ModalHelper.ShowModal<CardModal>(_cardById[_selectedCards[0]].Name, new() { { nameof(CardModal.CardId), _selectedCards[0] } });
        }

        public void Dispose()
        {
            _gridView?.Dispose();
        }
    }
}
