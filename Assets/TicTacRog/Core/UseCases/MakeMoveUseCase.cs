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

        public Result Execute(CellIndex targetCell, Symbol symbol)
        {
            var state = _boardRepository.GetCurrent();
            if (state == null)
                return Result.Failure("Game state not found.");

            if (state.Status != GameStatus.InProgress)
                return Result.Failure("Game is not in progress.");

            if (symbol == null)
                return Result.Failure("Symbol cannot be null.");

            var board = state.Board;
            if (!board.IsEmpty(targetCell))
                return Result.Failure("Cell is already occupied.");

            // Проверяем, что символ есть в руке текущего игрока
            var currentHand = state.GetCurrentHand();
            if (!currentHand.Contains(symbol))
                return Result.Failure("Symbol is not in player's hand.");

            // Проверяем, что тип символа соответствует текущему игроку
            if (symbol.Type != state.CurrentPlayerType)
                return Result.Failure("Symbol type does not match current player.");

            // Убираем символ из руки
            currentHand.RemoveSymbol(symbol);

            // Размещаем символ на поле
            board.SetSymbol(targetCell, symbol);

            // Записываем ход в историю
            state.History.AddMove(targetCell, symbol.Type);

            // Оцениваем состояние игры
            var status = _ruleSet.Evaluate(board, symbol.Type, targetCell);

            // Берем новые символы из деки, чтобы в руке стало 3
            var neededCount = currentHand.GetNeededCount();
            if (neededCount > 0)
            {
                var currentDeck = state.GetCurrentDeck();
                var drawnSymbols = currentDeck.Draw(neededCount);
                currentHand.AddSymbols(drawnSymbols);
            }

            _boardRepository.Save(state);
            _gameEvents.OnMoveMade(state, targetCell);

            if (status == GameStatus.InProgress)
            {
                state.SetStatus(status);
                state.SwitchPlayer();
            }
            else if (status == GameStatus.Win)
            {
                state.SetStatus(status, symbol.Type);
                var winningCells = _ruleSet.GetWinningCells(board, symbol.Type);
                _gameEvents.OnGameFinished(state, winningCells);
            }
            else
            {
                state.SetStatus(status);
                _gameEvents.OnGameFinished(state, System.Array.Empty<CellIndex>());
            }

            return Result.Success();
        }
    }
}