using TicTacRog.Core.Common;
using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public sealed class StartNewGameUseCase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGameEvents _gameEvents;

        public StartNewGameUseCase(IBoardRepository boardRepository, IGameEvents gameEvents)
        {
            _boardRepository = boardRepository;
            _gameEvents = gameEvents;
        }

        public Result Execute(int boardSize, Mark startingPlayer)
        {
            if (boardSize <= 0)
                return Result.Failure("Board size must be positive.");
            
            var board = new Board(boardSize);
            var state = new GameState(board, startingPlayer);
            _boardRepository.Save(state);
            _gameEvents.OnGameStarted(state);
            
            return Result.Success();
        }
    }
}