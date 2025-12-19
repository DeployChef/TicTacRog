using System.Collections.Generic;

namespace TicTacRog.Core.Domain
{
    public interface IGameRuleSet
    {
        GameStatus Evaluate(Board board, SymbolType lastSymbolType, CellIndex lastMove);
        IReadOnlyList<CellIndex> GetWinningCells(Board board, SymbolType winnerType);
    }
}