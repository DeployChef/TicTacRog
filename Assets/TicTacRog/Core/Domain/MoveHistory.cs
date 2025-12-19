using System.Collections.Generic;
using System.Linq;

namespace TicTacRog.Core.Domain
{
    /// <summary>
    /// Хранит историю всех ходов в игре.
    /// </summary>
    public sealed class MoveHistory
    {
        private readonly List<MoveRecord> _moves = new();

        public IReadOnlyList<MoveRecord> Moves => _moves;
        public int Count => _moves.Count;

        /// <summary>
        /// Добавляет ход в историю.
        /// </summary>
        public void AddMove(CellIndex cell, SymbolType playerType)
        {
            _moves.Add(new MoveRecord(cell, playerType));
        }

        /// <summary>
        /// Очищает всю историю.
        /// </summary>
        public void Clear()
        {
            _moves.Clear();
        }

        /// <summary>
        /// Получает последний ход (если есть).
        /// </summary>
        public MoveRecord? GetLastMove()
        {
            return _moves.Count > 0 ? _moves[_moves.Count - 1] : null;
        }

        /// <summary>
        /// Получает все ходы определенного игрока.
        /// </summary>
        public IEnumerable<MoveRecord> GetMovesByPlayer(SymbolType playerType)
        {
            return _moves.Where(m => m.PlayerType == playerType);
        }

        public override string ToString()
        {
            if (_moves.Count == 0) return "No moves";
            return string.Join(" → ", _moves.Select((m, i) => $"{i + 1}. {m}"));
        }
    }
}

