using System;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Events.Messages;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Presenters
{
    public sealed class GamePresenter
    {
        private readonly BoardView _boardView;
        private readonly StatusView _statusView;
        private readonly IMessageBus _bus;
        private readonly StartNewGameUseCase _startNewGame;
        private readonly MakeMoveUseCase _makeMove;
        private readonly IBoardRepository _repository;

        private IDisposable _startedSub;
        private IDisposable _movedSub;
        private IDisposable _finishedSub;

        public GamePresenter(
            BoardView boardView,
            StatusView statusView,
            IMessageBus bus,
            StartNewGameUseCase startNewGame,
            MakeMoveUseCase makeMove,
            IBoardRepository repository)
        {
            _boardView = boardView;
            _statusView = statusView;
            _bus = bus;
            _startNewGame = startNewGame;
            _makeMove = makeMove;
            _repository = repository;
        }

        public void Initialize()
        {
            BuildBoard();

            _startedSub = _bus.Subscribe<GameStartedMessage>(m => RedrawBoard(m.State));
            _movedSub = _bus.Subscribe<MoveMadeMessage>(m => RedrawBoard(m.State));
            _finishedSub = _bus.Subscribe<GameFinishedMessage>(m => RedrawBoard(m.State));

            _statusView.ResetButton.onClick.AddListener(OnResetClicked);
            RedrawBoard(_repository.GetCurrent());
        }

        public void Dispose()
        {
            _startedSub?.Dispose();
            _movedSub?.Dispose();
            _finishedSub?.Dispose();
            _statusView.ResetButton.onClick.RemoveListener(OnResetClicked);
        }

        private void OnResetClicked()
        {
            var size = _repository.GetCurrent().Board.Size;
            _startNewGame.Execute(size, Mark.Cross);
        }

        private void OnCellClicked(CellIndex index)
        {
            _makeMove.Execute(index);
            // перерисовка придёт через сообщения
        }

        private void OnGameStarted(GameStartedMessage evt)
        {
            RedrawBoard(evt.State);
        }

        private void OnMoveMade(MoveMadeMessage evt)
        {
            RedrawBoard(evt.State);
        }

        private void OnGameFinished(GameFinishedMessage evt)
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
    }
}
