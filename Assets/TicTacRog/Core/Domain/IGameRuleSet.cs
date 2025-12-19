using System.Collections.Generic;

namespace TicTacRog.Core.Domain
{
    public interface IGameRuleSet
    {
        GameStatus Evaluate(Board board, Mark lastMark, CellIndex lastMove);
        IReadOnlyList<CellIndex> GetWinningCells(Board board, Mark winner);
    }
}