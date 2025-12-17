using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
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

        private InMemoryBoardRepository _boardRepository;
        private Classic3x3RuleSet _ruleSet;
        private StartNewGameUseCase _startNewGame;
        private MakeMoveUseCase _makeMove;
        private GamePresenter _gamePresenter;

        public InMemoryBoardRepository BoardRepository => _boardRepository;
        public MakeMoveUseCase MakeMoveUseCase => _makeMove;

        private void Awake()
        {
            _boardRepository = new InMemoryBoardRepository();
            _ruleSet = new Classic3x3RuleSet();

            _startNewGame = new StartNewGameUseCase(_boardRepository);
            _makeMove = new MakeMoveUseCase(_boardRepository, _ruleSet);

            _startNewGame.Execute(_boardSize, _startingPlayer);

            _gamePresenter = new GamePresenter(_boardView, _makeMove, _boardRepository);
            _gamePresenter.Initialize();
        }
    }
}