using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure;
using TicTacRog.Presentation.Presenters;
using VContainer.Unity;

namespace TicTacRog.Presentation.DI
{
    public sealed class GameEntryPoint : IStartable
    {
        private readonly int _boardSize;
        private readonly Mark _startingPlayer;
        private readonly StartNewGameUseCase _startNewGame;
        private readonly GamePresenter _gamePresenter;
        private readonly BotListener _botListener;

        public GameEntryPoint(
            int boardSize,
            Mark startingPlayer,
            StartNewGameUseCase startNewGame,
            GamePresenter gamePresenter,
            BotListener botListener)
        {
            _boardSize = boardSize;
            _startingPlayer = startingPlayer;
            _startNewGame = startNewGame;
            _gamePresenter = gamePresenter;
            _botListener = botListener;
        }

        public void Start()
        {
            _startNewGame.Execute(_boardSize, _startingPlayer);
            _gamePresenter.Initialize();
            _botListener.Initialize();
        }
    }
}