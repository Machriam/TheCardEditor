using Microsoft.AspNetCore.Components;
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

        public string Name { get; set; } = "";
    }

    public partial class CardEditor : IDisposable
    {
        private const string GridId = "CardGrid";
        [Inject] private IGridViewFactory GridViewFactory { get; set; } = default!;
        [Inject] private ServiceAccessor<CardService> CardService { get; set; } = default!;
        [Inject] private ApplicationStorage ApplicationStorage { get; set; } = default!;
        private IGridView _gridView = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _gridView = GridViewFactory.CreateGrid(this, GridId, OnCardsSelected);
                if (ApplicationStorage.SelectedCardSet == null) return;
                var cards = CardService.Execute(cs => cs.GetCards(ApplicationStorage.SelectedCardSet.Id))
                    .Select(c => new CardGridModel(c.Id)
                    {
                        Name = c.Name
                    });
                await _gridView.UpdateGrid(new DisplayGridModel<CardGridModel>(cards));
            }
            base.OnAfterRender(firstRender);
        }

        private void OnCardsSelected(long[] id)
        {
        }

        public void Dispose()
        {
            _gridView?.Dispose();
        }
    }
}
