using System.Collections.Generic;
using System.Linq;

namespace TicTacRog.Core.Domain
{
    /// <summary>
    /// Рука игрока - текущие символы, доступные для размещения на поле.
    /// </summary>
    public sealed class Hand
    {
        private readonly List<Symbol> _symbols;
        private const int MaxHandSize = 3;

        public int Count => _symbols.Count;
        public bool IsEmpty => _symbols.Count == 0;
        public IReadOnlyList<Symbol> Symbols => _symbols.AsReadOnly();

        public Hand()
        {
            _symbols = new List<Symbol>();
        }

        /// <summary>
        /// Добавить символы в руку (из деки).
        /// </summary>
        public void AddSymbols(IEnumerable<Symbol> symbols)
        {
            if (symbols == null) return;

            foreach (var symbol in symbols)
            {
                if (symbol != null && _symbols.Count < MaxHandSize)
                {
                    _symbols.Add(symbol);
                }
            }
        }

        /// <summary>
        /// Убрать символ из руки (после использования на поле).
        /// </summary>
        public bool RemoveSymbol(Symbol symbol)
        {
            return _symbols.Remove(symbol);
        }

        /// <summary>
        /// Проверить, есть ли символ в руке.
        /// </summary>
        public bool Contains(Symbol symbol)
        {
            return _symbols.Contains(symbol);
        }

        /// <summary>
        /// Сколько символов нужно взять из деки, чтобы заполнить руку до максимума.
        /// </summary>
        public int GetNeededCount()
        {
            return MaxHandSize - _symbols.Count;
        }
    }
}
