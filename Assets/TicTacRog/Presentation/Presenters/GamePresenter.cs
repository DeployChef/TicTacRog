using System;
using System.Collections.Generic;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Events.Messages;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Views;
using TicTacRog.Presentation.Animation;
using TicTacRog.Presentation.StateMachine;

namespace TicTacRog.Presentation.Presenters
{
    public sealed class GamePresenter : IDisposable
    {
        private readonly BoardView _boardView;
        private readonly StatusView _statusView;
        private readonly HandView _handView;
        private readonly IMessageBus _bus;
        private readonly StartNewGameUseCase _startNewGame;
        private readonly MakeMoveUseCase _makeMove;
        private readonly IBoardRepository _repository;
        private readonly AnimationQueue _animationQueue;
        private readonly GameFlowStateMachine _stateMachine;
        private readonly BoardBuilder _boardBuilder;
        private readonly StatusTextFormatter _statusFormatter;


        private IDisposable _startedSub;
        private IDisposable _movedSub;
        private IDisposable _finishedSub;

        public GamePresenter(
            BoardView boardView,
            StatusView statusView,
            HandView handView,
            IMessageBus bus,
            StartNewGameUseCase startNewGame,
            MakeMoveUseCase makeMove,
            IBoardRepository repository,
            AnimationQueue animationQueue,
            GameFlowStateMachine stateMachine,
            BoardBuilder boardBuilder,
            StatusTextFormatter statusFormatter)
        {
            _boardView = boardView ?? throw new ArgumentNullException(nameof(boardView));
            _statusView = statusView ?? throw new ArgumentNullException(nameof(statusView));
            _handView = handView ?? throw new ArgumentNullException(nameof(handView));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _startNewGame = startNewGame ?? throw new ArgumentNullException(nameof(startNewGame));
            _makeMove = makeMove ?? throw new ArgumentNullException(nameof(makeMove));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _animationQueue = animationQueue ?? throw new ArgumentNullException(nameof(animationQueue));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _boardBuilder = boardBuilder ?? throw new ArgumentNullException(nameof(boardBuilder));
            _statusFormatter = statusFormatter ?? throw new ArgumentNullException(nameof(statusFormatter));
        }

        public void Initialize()
        {
            _boardBuilder.BuildBoard(OnCellDropped);

            _startedSub = _bus.Subscribe<GameStartedMessage>(OnGameStarted);
            _movedSub = _bus.Subscribe<MoveMadeMessage>(OnMoveMade);
            _finishedSub = _bus.Subscribe<GameFinishedMessage>(OnGameFinished);

            _stateMachine.OnStateChanged += OnStateChanged;
            _stateMachine.OnEnteredWaitingForPlayer += OnEnteredWaitingForPlayer;
            _stateMachine.OnEnteredBotThinking += OnEnteredBotThinking;
            _stateMachine.OnEnteredGameFinished += OnEnteredGameFinished;

            _animationQueue.OnEventStarted += OnAnimationStarted;
            _animationQueue.OnEventCompleted += OnAnimationCompleted;

            _stateMachine.Initialize();

            _statusView.ResetButton.onClick.AddListener(OnResetClicked);
            _statusView.ResetButton.interactable = true;
            
            var currentState = _repository.GetCurrent();
            if (currentState != null)
            {
                RedrawBoardImmediate(currentState);
                UpdateHandView(currentState);
            }
            else
            {
                Debug.LogWarning("[GamePresenter] Initial state is null, skipping redraw. " +
                    "Board will be drawn when GameStartedMessage arrives.");
            }
        }

        public void Dispose()
        {
            _startedSub?.Dispose();
            _movedSub?.Dispose();
            _finishedSub?.Dispose();
            
            _stateMachine.OnStateChanged -= OnStateChanged;
            _stateMachine.OnEnteredWaitingForPlayer -= OnEnteredWaitingForPlayer;
            _stateMachine.OnEnteredBotThinking -= OnEnteredBotThinking;
            _stateMachine.OnEnteredGameFinished -= OnEnteredGameFinished;
            _stateMachine.Dispose();

            _animationQueue.OnEventStarted -= OnAnimationStarted;
            _animationQueue.OnEventCompleted -= OnAnimationCompleted;
            
            _statusView.ResetButton.onClick.RemoveListener(OnResetClicked);
        }

        #region UI Events

        private void OnResetClicked()
        {
            var currentState = _stateMachine.CurrentState;
            
            if (currentState != GameFlowState.WaitingForPlayerInput && 
                currentState != GameFlowState.GameFinished)
            {
                Debug.Log($"Cannot reset: current state is {currentState}");
                return;
            }
            
            Debug.Log("[GamePresenter] Reset button clicked");
            
            _animationQueue.Clear();
            
            foreach (var cellView in _boardBuilder.CellViews.Values)
            {
                cellView.SetSymbolImmediate(null);
            }
            
            var size = _repository.GetCurrent().Board.Size;
            var result = _startNewGame.Execute(size, SymbolType.Cross);
            if (!result.IsSuccess)
            {
                Debug.LogError($"[GamePresenter] Failed to start new game: {result.ErrorMessage}");
            }
        }

        private void OnSymbolCardDropped(Symbol symbol, CellIndex? cellIndex)
        {
            // Если drop был на ячейку - делаем ход
            if (cellIndex.HasValue)
            {
                TryMakeMoveWithSymbol(cellIndex.Value, symbol);
            }
            // Если drop не на ячейку - просто игнорируем (карточка вернется на место)
        }

        private void OnCellDropped(CellIndex index, Symbol symbol)
        {
            // Обработка drop через IDropHandler в CellView
            TryMakeMoveWithSymbol(index, symbol);
        }

        private void TryMakeMoveWithSymbol(CellIndex index, Symbol symbol)
        {
            if (!_stateMachine.CanPlayerMove())
            {
                Debug.Log($"Cannot move: current state is {_stateMachine.CurrentState}");
                return;
            }

            var state = _repository.GetCurrent();

            if (state.CurrentPlayerType != SymbolType.Cross)
            {
                Debug.Log("Not player's turn");
                return;
            }

            if (!state.Board.IsEmpty(index))
            {
                if (_boardBuilder.CellViews.TryGetValue(index, out var cellView))
                {
                    cellView.PlayErrorShake();
                }
                return;
            }

            var playerHand = state.PlayerHand;
            if (!playerHand.Contains(symbol))
            {
                Debug.LogWarning("[GamePresenter] Symbol is not in player's hand");
                return;
            }

            var result = _makeMove.Execute(index, symbol);

            if (!result.IsSuccess)
            {
                Debug.LogError($"[GamePresenter] Failed to make move: {result.ErrorMessage}");
                if (_boardBuilder.CellViews.TryGetValue(index, out var cellView))
                {
                    cellView.PlayErrorShake();
                }
                return;
            }

            _stateMachine.OnPlayerMoved();
        }

        #endregion

        #region Domain Events

        private void OnGameStarted(GameStartedMessage evt)
        {
            Debug.Log("[Presenter] Game started");
            
            _animationQueue.Clear();
            RedrawBoardImmediate(evt.State);
            UpdateHandView(evt.State);
            _stateMachine.OnGameStarted();
        }

        private void OnMoveMade(MoveMadeMessage msg)
        {
            Debug.Log($"[Presenter] Move made at {msg.LastMove.Row},{msg.LastMove.Column}");
            
            if (_boardBuilder.CellViews.TryGetValue(msg.LastMove, out var cellView))
            {
                var symbol = msg.State.Board.GetSymbol(msg.LastMove);
                var animEvent = new MoveAnimationEvent(cellView, symbol, msg.LastMove);
                _animationQueue.Enqueue(animEvent);
            }
        }

        private void OnGameFinished(GameFinishedMessage evt)
        {
            Debug.Log($"[Presenter] Game finished: {evt.State.Status}");
            
            if (evt.State.Status == GameStatus.Win && evt.WinningCells.Count > 0)
            {
                foreach (var cellIndex in evt.WinningCells)
                {
                    if (_boardBuilder.CellViews.TryGetValue(cellIndex, out var cellView))
                    {
                        var winEvent = new WinHighlightAnimationEvent(cellView);
                        _animationQueue.Enqueue(winEvent);
                    }
                }
            }
            
            _stateMachine.OnGameFinished();
        }

        #endregion

        #region State Machine Events

        private void OnStateChanged(GameFlowState newState, GameFlowState oldState)
        {
            Debug.Log($"[Presenter] State changed: {oldState} → {newState}");
            UpdateUI(newState);
        }

        private void OnEnteredWaitingForPlayer()
        {
            Debug.Log("[Presenter] Waiting for player input");
            SetCellsInteractionEnabled(true);
        }

        private void OnEnteredBotThinking()
        {
            Debug.Log("[Presenter] Bot is thinking...");
            SetCellsInteractionEnabled(false);
        }

        private void OnEnteredGameFinished()
        {
            Debug.Log("[Presenter] Game finished");
            
            SetCellsInteractionEnabled(false);
            _statusView.ResetButton.interactable = true;
        }

        #endregion

        #region Animation Events

        private void OnAnimationStarted(IAnimationEvent evt)
        {
            Debug.Log($"[Presenter] Animation started: {evt.GetType().Name}");
        }

        private void OnAnimationCompleted(IAnimationEvent evt)
        {
            Debug.Log($"[Presenter] Animation completed: {evt.GetType().Name}");
            
            var state = _repository.GetCurrent();
            UpdateStatusText(state, _stateMachine.CurrentState);
            UpdateHandView(state);
        }

        #endregion

        #region Board Building

        private void RedrawBoardImmediate(GameState state)
        {
            _boardBuilder.RedrawBoardImmediate(state);
            UpdateStatusText(state, _stateMachine.CurrentState);
        }

        #endregion

        #region UI Updates

        private void UpdateUI(GameFlowState flowState)
        {
            var state = _repository.GetCurrent();
            UpdateStatusText(state, flowState);
        }

        private void SetCellsInteractionEnabled(bool enabled)
        {
            // Кнопки отключены, так как используем только drop
            // Метод оставлен для совместимости, но ничего не делает
        }

        private void UpdateStatusText(GameState state, GameFlowState flowState)
        {
            string statusText = _statusFormatter.GetStatusText(state, flowState);
            _statusView.StatusText.text = statusText;
        }

        private void UpdateHandView(GameState state)
        {
            if (_handView == null) return;

            // Обновляем руку только для игрока (Cross)
            if (state.CurrentPlayerType == SymbolType.Cross)
            {
                _handView.UpdateHand(state.PlayerHand.Symbols, OnSymbolCardDropped);
            }
        }

        #endregion
    }
}

