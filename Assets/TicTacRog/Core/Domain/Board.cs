using System;

namespace TicTacRog.Core.Domain
{
    public class Board
    {
        public int Size { get; }
        private readonly Mark[] _cells;

        public Board(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            Size = size;
            _cells = new Mark[size * size];
        }

        public Mark GetMark(CellIndex index)
        {
            return _cells[ToFlatIndex(index)];
        }

        public void SetMark(CellIndex index, Mark mark)
        {
            _cells[ToFlatIndex(index)] = mark;
        }

        public CellState GetCellState(CellIndex index)
        {
            return new CellState(index, GetMark(index));
        }

        public bool IsEmpty(CellIndex index)
        {
            return GetMark(index) == Mark.None;
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