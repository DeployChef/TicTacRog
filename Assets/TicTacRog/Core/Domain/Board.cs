using System;
using System.Collections.Generic;

namespace TicTacRog.Core.Domain
{
    public class Board
    {
        public int Size { get; }
        private readonly Symbol[] _cells;

        public Board(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            Size = size;
            _cells = new Symbol[size * size];
        }

        public Symbol GetSymbol(CellIndex index)
        {
            return _cells[ToFlatIndex(index)];
        }

        public void SetSymbol(CellIndex index, Symbol symbol)
        {
            _cells[ToFlatIndex(index)] = symbol;
        }

        public CellState GetCellState(CellIndex index)
        {
            return new CellState(index, GetSymbol(index));
        }

        public bool IsEmpty(CellIndex index)
        {
            return GetSymbol(index) == null;
        }

        /// <summary>
        /// Получить все символы, размещенные на доске (для возврата в деку после партии).
        /// </summary>
        public List<Symbol> GetAllSymbols()
        {
            var symbols = new List<Symbol>();
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    symbols.Add(_cells[i]);
                }
            }
            return symbols;
        }

        /// <summary>
        /// Очистить доску и вернуть все символы.
        /// </summary>
        public List<Symbol> ClearAndReturnSymbols()
        {
            var symbols = GetAllSymbols();
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = null;
            }
            return symbols;
        }

        private int ToFlatIndex(CellIndex index)
        {
            if (index.Row < 0 || index.Row >= Size)
                throw new ArgumentOutOfRangeException(nameof(index.Row), $"Row {index.Row} is out of bounds (0..{Size - 1}).");
            
            if (index.Column < 0 || index.Column >= Size)
                throw new ArgumentOutOfRangeException(nameof(index.Column), $"Column {index.Column} is out of bounds (0..{Size - 1}).");
            
            return index.Row * Size + index.Column;
        }
    }
}