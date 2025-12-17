using TicTacRog.Core.Domain;

namespace TicTacRog.Infrastructure.Events
{
    public readonly struct MoveMadeEvent
    {
        public GameState State { get; }
        public CellIndex LastMove { get; }
        public MoveMadeEvent(GameState state, CellIndex lastMove)
        {
            State = state;
            LastMove = lastMove;
        }
    }
}
