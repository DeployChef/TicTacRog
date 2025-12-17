namespace TicTacRog.Core.Domain
{
    public interface IGameRuleSet
    {
        GameStatus Evaluate(Board board, Mark lastMark, CellIndex lastMove);
    }
}