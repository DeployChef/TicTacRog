using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public sealed class StartNewGameUseCase
    {
        private readonly IBoardRepository _boardRepository;

        public StartNewGameUseCase(IBoardRepository boardRepository)
        {
            _boardRepository = boardRepository;
        }

        public void Execute(int boardSize, Mark startingPlayer)
        {
            var board = new Board(boardSize);
            var state = new GameState(board, startingPlayer);
            _boardRepository.Save(state);
        }
    }
}