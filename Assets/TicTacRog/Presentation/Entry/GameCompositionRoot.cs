using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Views;
using TicTacRog.Presentation.Presenters;

namespace TicTacRog.Presentation.Entry
{
    public sealed class GameCompositionRoot : MonoBehaviour
    {
        [SerializeField] private int _boardSize = 3;
        [SerializeField] private Mark _startingPlayer = Mark.Cross;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private StatusView _statusView;

        private EventBus _eventBus;
        private GameEventsAdapter _gameEvents;
        private InMemoryBoardRepository _boardRepository;
        private Classic3x3RuleSet _ruleSet;
        private StartNewGameUseCase _startNewGame;
        private MakeMoveUseCase _makeMove;
        private GamePresenter _gamePresenter;

        private void Awake()
        {
            _eventBus = new EventBus();
            _gameEvents = new GameEventsAdapter(_eventBus);

            _boardRepository = new InMemoryBoardRepository();
            _ruleSet = new Classic3x3RuleSet();

            _startNewGame = new StartNewGameUseCase(_boardRepository, _gameEvents);
            _makeMove = new MakeMoveUseCase(_boardRepository, _ruleSet, _gameEvents);

            _startNewGame.Execute(_boardSize, _startingPlayer);

            _gamePresenter = new GamePresenter(
                _boardView,
                _statusView,
                _eventBus,
                _startNewGame,
                _makeMove,
                _boardRepository);

            _gamePresenter.Initialize();
        }
    }
}