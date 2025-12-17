namespace TicTacRog.Core.Domain
{
    public readonly struct CellIndex
    {
        public int Row { get; }
        public int Column { get; }

        public CellIndex(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString()
        {
            return $"({Row}, {Column})";
        }
    }
}