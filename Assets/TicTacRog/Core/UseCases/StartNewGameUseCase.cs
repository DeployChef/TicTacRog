// обновлённый StartNewGameUseCase

using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;

public sealed class StartNewGameUseCase
{
    private readonly IBoardRepository _boardRepository;
    private readonly IGameEvents _gameEvents;

    public StartNewGameUseCase(IBoardRepository boardRepository, IGameEvents gameEvents)
    {
        _boardRepository = boardRepository;
        _gameEvents = gameEvents;
    }

    public void Execute(int boardSize, Mark startingPlayer)
    {
        var board = new Board(boardSize);
        var state = new GameState(board, startingPlayer);
        _boardRepository.Save(state);
        _gameEvents.OnGameStarted(state);
    }
}