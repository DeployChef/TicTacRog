using System.Collections.Generic;
using TicTacRog.Core.Domain;

namespace TicTacRog.Infrastructure.Events.Messages
{
    public readonly struct GameFinishedMessage
    {
        public GameState State { get; }
        public IReadOnlyList<CellIndex> WinningCells { get; }
        
        public GameFinishedMessage(GameState state, IReadOnlyList<CellIndex> winningCells)
        {
            State = state;
            WinningCells = winningCells;
        }
    }
}
