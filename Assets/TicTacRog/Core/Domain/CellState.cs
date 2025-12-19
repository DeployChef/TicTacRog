namespace TicTacRog.Core.Domain
{
    public readonly struct CellState
    {
        public CellIndex Index { get; }
        public Symbol Symbol { get; }

        public CellState(CellIndex index, Symbol symbol)
        {
            Index = index;
            Symbol = symbol;
        }
    }
}