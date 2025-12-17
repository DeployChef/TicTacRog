using TicTacRog.Core.Domain;

namespace TicTacRog.Infrastructure.Events.Messages
{
    public readonly struct MoveMadeMessage
    {
        public GameState State { get; }
        public CellIndex LastMove { get; }
        public MoveMadeMessage(GameState state, CellIndex lastMove)
        {
            State = state;
            LastMove = lastMove;
        }
    }
}
