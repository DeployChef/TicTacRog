using System.Collections.Generic;
using System.Linq;

namespace TicTacRog.Core.Domain
{
    /// <summary>
    /// Дека символов игрока. При старте игры содержит начальный набор символов.
    /// </summary>
    public sealed class Deck
    {
        private readonly List<Symbol> _symbols;

        public int Count => _symbols.Count;
        public bool IsEmpty => _symbols.Count == 0;

        public Deck(IEnumerable<Symbol> initialSymbols)
        {
            _symbols = new List<Symbol>(initialSymbols ?? Enumerable.Empty<Symbol>());
        }

        /// <summary>
        /// Взять указанное количество символов из деки.
        /// </summary>
        /// <param name="count">Количество символов для взятия</param>
        /// <returns>Список взятых символов</returns>
        public List<Symbol> Draw(int count)
        {
            if (count <= 0)
                return new List<Symbol>();

            var drawn = new List<Symbol>();
            var actualCount = System.Math.Min(count, _symbols.Count);

            for (int i = 0; i < actualCount; i++)
            {
                drawn.Add(_symbols[0]);
                _symbols.RemoveAt(0);
            }

            return drawn;
        }

        /// <summary>
        /// Вернуть символы в деку (например, после окончания партии).
        /// </summary>
        public void ReturnSymbols(IEnumerable<Symbol> symbols)
        {
            if (symbols == null) return;

            foreach (var symbol in symbols)
            {
                if (symbol != null)
                {
                    _symbols.Add(symbol);
                }
            }
        }
    }
}
