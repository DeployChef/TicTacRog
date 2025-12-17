namespace TicTacRog.Core.Domain
{
    public readonly struct CellState
    {
        public CellIndex Index { get; }
        public Mark Mark { get; }

        public CellState(CellIndex index, Mark mark)
        {
            Index = index;
            Mark = mark;
        }
    }
}