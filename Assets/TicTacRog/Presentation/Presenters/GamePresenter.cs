using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Presenters
{
    public sealed class GamePresenter
    {
        private readonly BoardView _boardView;
        private readonly StatusView _statusView;
        private readonly EventBus _eventBus;
        private readonly StartNewGameUseCase _startNewGameUseCase;
        private readonly MakeMoveUseCase _makeMoveUseCase;
        private readonly IBoardRepository _repository;

        public GamePresenter(
            BoardView boardView,
            StatusView statusView,
            EventBus eventBus,
            StartNewGameUseCase startNewGameUseCase,
            MakeMoveUseCase makeMoveUseCase,
            IBoardRepository repository)
        {
            _boardView = boardView;
            _statusView = statusView;
            _eventBus = eventBus;
            _startNewGameUseCase = startNewGameUseCase;
            _makeMoveUseCase = makeMoveUseCase;
            _repository = repository;
        }

        public void Initialize()
        {
            BuildBoard();

            _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Subscribe<MoveMadeEvent>(OnMoveMade);
            _eventBus.Subscribe<GameFinishedEvent>(OnGameFinished);

            _statusView.ResetButton.onClick.AddListener(OnResetClicked);

            RedrawBoard(_repository.GetCurrent());
        }

        private void OnGameStarted(GameStartedEvent evt)
        {
            RedrawBoard(evt.State);
        }

        private void OnMoveMade(MoveMadeEvent evt)
        {
            RedrawBoard(evt.State);
        }

        private void OnGameFinished(GameFinishedEvent evt)
        {
            RedrawBoard(evt.State);
        }

        private void BuildBoard()
        {
            var state = _repository.GetCurrent();
            var size = state.Board.Size;

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    var index = new CellIndex(row, col);
                    var go = UnityEngine.Object.Instantiate(_boardView.CellPrefab, _boardView.CellsRoot);
                    var view = go.GetComponent<CellView>();
                    view.Init(index, OnCellClicked);
                }
            }
        }

        private void OnCellClicked(CellIndex index)
        {
            _makeMoveUseCase.Execute(index);
        }

        private void RedrawBoard(GameState state)
        {
            var cells = _boardView.CellsRoot.GetComponentsInChildren<CellView>();

            foreach (var cell in cells)
            {
                var idx = cell.transform.GetSiblingIndex();
                var size = state.Board.Size;
                var row = idx / size;
                var col = idx % size;
                var mark = state.Board.GetMark(new CellIndex(row, col));
                cell.SetMark(mark);
            }

            UpdateStatusText(state);
        }

        private void UpdateStatusText(GameState state)
        {
            switch (state.Status)
            {
                case GameStatus.InProgress:
                    _statusView.StatusText.text = state.CurrentPlayer == Mark.Cross ? "Turn: X" : "Turn: O";
                    break;
                case GameStatus.Win:
                    _statusView.StatusText.text = state.CurrentPlayer == Mark.Cross ? "Win: X" : "Win: O";
                    break;
                case GameStatus.Draw:
                    _statusView.StatusText.text = "Draw";
                    break;
            }
        }

        private void OnResetClicked()
        {
            var size = _repository.GetCurrent().Board.Size;
            _startNewGameUseCase.Execute(size, Mark.Cross);
        }
    }
}
