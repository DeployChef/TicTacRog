using System.Collections.Generic;
using UnityEngine;
using TicTacRog.Core.Domain;

namespace TicTacRog.Presentation.Views
{
    /// <summary>
    /// Представление руки игрока - контейнер для карточек символов.
    /// </summary>
    public sealed class HandView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private GameObject cardPrefab;

        private readonly List<SymbolCardView> _cardViews = new List<SymbolCardView>();

        public Transform CardsRoot => cardsRoot;
        public GameObject CardPrefab => cardPrefab;
        public IReadOnlyList<SymbolCardView> CardViews => _cardViews;

        public void Clear()
        {
            foreach (var cardView in _cardViews)
            {
                if (cardView != null)
                {
                    Destroy(cardView.gameObject);
                }
            }
            _cardViews.Clear();
        }

        public void AddCard(Symbol symbol, System.Action<Symbol, CellIndex?> onCardDropped)
        {
            if (cardPrefab == null || cardsRoot == null)
            {
                Debug.LogError("[HandView] CardPrefab or CardsRoot is not assigned!");
                return;
            }

            var cardObject = Instantiate(cardPrefab, cardsRoot);
            var cardView = cardObject.GetComponent<SymbolCardView>();
            
            if (cardView == null)
            {
                Debug.LogError("[HandView] SymbolCardView component not found on card prefab!");
                Destroy(cardObject);
                return;
            }

            cardView.Init(symbol, onCardDropped);
            _cardViews.Add(cardView);
        }

        public void RemoveCard(Symbol symbol)
        {
            for (int i = _cardViews.Count - 1; i >= 0; i--)
            {
                var cardView = _cardViews[i];
                if (cardView != null && cardView.Symbol == symbol)
                {
                    Destroy(cardView.gameObject);
                    _cardViews.RemoveAt(i);
                    return;
                }
            }
        }

        public void UpdateHand(IReadOnlyList<Symbol> symbols, System.Action<Symbol, CellIndex?> onCardDropped)
        {
            Clear();
            
            if (symbols == null) return;

            foreach (var symbol in symbols)
            {
                AddCard(symbol, onCardDropped);
            }
        }
    }
}
