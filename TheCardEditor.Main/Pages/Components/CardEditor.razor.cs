using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.Core.Grid;
using TheCardEditor.Services;
using TheCardEditor.Shared;

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

        [GridMetaData(Resizable = true, Hide = true)]
        [JsonExtensionData]
        public Dictionary<string, object> TagTexts { get; set; } = new();
    }

    public partial class CardEditor : IDisposable
    {
        private const string GridId = "CardGrid";
        [Inject] private IGridViewFactory GridViewFactory { get; set; } = default!;
        [Inject] private ServiceAccessor<CardService> CardService { get; set; } = default!;
        [Inject] private ApplicationStorage ApplicationStorage { get; set; } = default!;
        [Inject] private IModalHelper ModalHelper { get; set; } = default!;
        private long[] _selectedCards = Array.Empty<long>();
        private Dictionary<long, CardGridModel> _cardById = new();
        private List<string> Tags { get; set; } = new();
        private IGridView _gridView = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _gridView = GridViewFactory.CreateGrid(this, GridId, OnCardsSelected);
                await UpdateGrid();
            }
            base.OnAfterRender(firstRender);
        }

        public async Task UpdateGrid()
        {
            if (ApplicationStorage.SelectedCardSet == null) return;
            var cards = CardService.Execute(cs => cs.GetCards(ApplicationStorage.SelectedCardSet.Id));
            Tags = cards.SelectMany(c => c.SerializedData().GetTags().Select(t => t.Tag)).Distinct().OrderBy(t => t).ToList();
            _cardById = cards.Select(c =>
            {
                var tagTextsByTag = c.SerializedData().GetTags().ToDictionary(t => t.Tag, t => t.Text);
                return new CardGridModel(c.Id)
                {
                    Name = c.Name,
                    Data = c.Data,
                    TagTexts = Tags.ToDictionary(t => t, t => (object)(tagTextsByTag.TryGetValue(t, out var tag) ? tag : "")),
                };
            }).ToDictionary(c => c.Id);
            await _gridView.UpdateGrid(new DisplayGridModel<CardGridModel>(_cardById.Values,
                dynamicColumns: new() { { nameof(CardGridModel.TagTexts), Tags } }));
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
                { nameof(CardModal.CardId), null },
                { nameof(CardModal.Tags), Tags},
            }, disableBackgroundCancel: true);
            await UpdateGrid();
        }

        public async Task EditCards()
        {
            if (_selectedCards.Length == 0) return;
            await ModalHelper.ShowModal<CardModal>(
                _cardById[_selectedCards[0]].Name, new() {
                    { nameof(CardModal.CardId), _selectedCards[0] },
                    {nameof(CardModal.Tags),Tags } },
                disableBackgroundCancel: true, hideCloseButton: true);
            await UpdateGrid();
        }

        public void Dispose()
        {
            _gridView?.Dispose();
        }
    }
}
