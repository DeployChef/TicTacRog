namespace TicTacRog.Core.Domain
{
    /// <summary>
    /// Запись о сделанном ходе.
    /// </summary>
    public readonly struct MoveRecord
    {
        public CellIndex Cell { get; }
        public Mark Player { get; }

        public MoveRecord(CellIndex cell, Mark player)
        {
            Cell = cell;
            Player = player;
        }

        public override string ToString() => $"{Player} at ({Cell.Row},{Cell.Column})";
    }
}

