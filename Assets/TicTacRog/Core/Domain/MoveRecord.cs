namespace TicTacRog.Core.Domain
{
    /// <summary>
    /// Запись о сделанном ходе.
    /// </summary>
    public readonly struct MoveRecord
    {
        public CellIndex Cell { get; }
        public SymbolType PlayerType { get; }

        public MoveRecord(CellIndex cell, SymbolType playerType)
        {
            Cell = cell;
            PlayerType = playerType;
        }

        public override string ToString() => $"{PlayerType} at ({Cell.Row},{Cell.Column})";
    }
}

