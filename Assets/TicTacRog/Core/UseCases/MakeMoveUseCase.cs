using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public sealed class MakeMoveUseCase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGameRuleSet _ruleSet;

        public MakeMoveUseCase(IBoardRepository boardRepository, IGameRuleSet ruleSet)
        {
            _boardRepository = boardRepository;
            _ruleSet = ruleSet;
        }

        public void Execute(CellIndex targetCell)
        {
            var state = _boardRepository.GetCurrent();
            if (state.Status != GameStatus.InProgress)
                return;

            var board = state.Board;

            if (!board.IsEmpty(targetCell))
                return; // позже можно вернуть результат/ошибку

            board.SetMark(targetCell, state.CurrentPlayer);

            var status = _ruleSet.Evaluate(board, state.CurrentPlayer, targetCell);
            state.SetStatus(status);

            if (status == GameStatus.InProgress)
            {
                state.SwitchPlayer();
            }

            _boardRepository.Save(state);
        }
    }
}