using TicTacRog.Core.Common;
using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public sealed class MakeMoveUseCase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGameRuleSet _ruleSet;
        private readonly IGameEvents _gameEvents;

        public MakeMoveUseCase(IBoardRepository boardRepository, IGameRuleSet ruleSet, IGameEvents gameEvents)
        {
            _boardRepository = boardRepository;
            _ruleSet = ruleSet;
            _gameEvents = gameEvents;
        }

        public Result Execute(CellIndex targetCell)
        {
            var state = _boardRepository.GetCurrent();
            if (state == null)
                return Result.Failure("Game state not found.");

            if (state.Status != GameStatus.InProgress)
                return Result.Failure("Game is not in progress.");

            var board = state.Board;
            if (!board.IsEmpty(targetCell))
                return Result.Failure("Cell is already occupied.");

            var currentPlayer = state.CurrentPlayer;
            board.SetMark(targetCell, currentPlayer);

            // Записываем ход в историю
            state.History.AddMove(targetCell, currentPlayer);

            var status = _ruleSet.Evaluate(board, currentPlayer, targetCell);

            // Устанавливаем статус и победителя
            if (status == GameStatus.Win)
            {
                state.SetStatus(status, currentPlayer);  // Победитель = текущий игрок
            }
            else
            {
                state.SetStatus(status);
            }

            if (status == GameStatus.InProgress)
            {
                state.SwitchPlayer();
            }
            else
            {
                _gameEvents.OnGameFinished(state);
            }

            _boardRepository.Save(state);
            _gameEvents.OnMoveMade(state, targetCell);

            return Result.Success();
        }
    }
}