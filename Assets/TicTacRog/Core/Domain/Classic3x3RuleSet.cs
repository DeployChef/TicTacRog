namespace TicTacRog.Core.Domain
{
    public sealed class Classic3x3RuleSet : IGameRuleSet
    {
        public GameStatus Evaluate(Board board, Mark lastMark, CellIndex lastMove)
        {
            // Логику победы/ничьей допишем отдельно
            return GameStatus.InProgress;
        }
    }
}