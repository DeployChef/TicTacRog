using System.Collections.Generic;
using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public interface IGameEvents
    {
        void OnGameStarted(GameState state);
        void OnMoveMade(GameState state, CellIndex lastMove);
        void OnGameFinished(GameState state, IReadOnlyList<CellIndex> winningCells);
    }
}