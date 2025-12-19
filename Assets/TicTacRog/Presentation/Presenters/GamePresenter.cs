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
    /// <summary>
    /// Презентер игры с State Machine и очередью анимаций
    /// ФИНАЛЬНАЯ ВЕРСИЯ для продакшна
    /// </summary>
    public sealed class GamePresenter : IDisposable
    {
        private readonly BoardView _boardView;
        private readonly StatusView _statusView;
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
            _boardBuilder.BuildBoard(OnCellClicked);

            // Подписываемся на доменные события
            _startedSub = _bus.Subscribe<GameStartedMessage>(OnGameStarted);
            _movedSub = _bus.Subscribe<MoveMadeMessage>(OnMoveMade);
            _finishedSub = _bus.Subscribe<GameFinishedMessage>(OnGameFinished);

            // Подписываемся на изменения состояния State Machine
            _stateMachine.OnStateChanged += OnStateChanged;
            _stateMachine.OnEnteredWaitingForPlayer += OnEnteredWaitingForPlayer;
            _stateMachine.OnEnteredBotThinking += OnEnteredBotThinking;
            _stateMachine.OnEnteredGameFinished += OnEnteredGameFinished;

            // Подписываемся на события анимаций
            _animationQueue.OnEventStarted += OnAnimationStarted;
            _animationQueue.OnEventCompleted += OnAnimationCompleted;

            _stateMachine.Initialize();

            _statusView.ResetButton.onClick.AddListener(OnResetClicked);
            
            // Кнопка Reset всегда доступна!
            _statusView.ResetButton.interactable = true;
            
            // Перерисовываем доску только если состояние уже создано
            var currentState = _repository.GetCurrent();
            if (currentState != null)
            {
                RedrawBoardImmediate(currentState);
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
            
            // Разрешаем Reset только когда:
            // 1. Ждем хода игрока (WaitingForPlayerInput)
            // 2. Игра закончена (GameFinished)
            // Блокируем во время анимаций и хода бота
            if (currentState != GameFlowState.WaitingForPlayerInput && 
                currentState != GameFlowState.GameFinished)
            {
                Debug.Log($"Cannot reset: current state is {currentState}");
                return;
            }
            
            Debug.Log("[GamePresenter] Reset button clicked");
            
            // 1. Останавливаем все анимации
            _animationQueue.Clear();
            
            // 2. Принудительно сбрасываем все клетки (на случай если анимация не успела остановиться)
            foreach (var cellView in _boardBuilder.CellViews.Values)
            {
                cellView.SetMarkImmediate(Mark.None);
            }
            
            // 3. Запускаем новую игру
            var size = _repository.GetCurrent().Board.Size;
            var result = _startNewGame.Execute(size, Mark.Cross);
            if (!result.IsSuccess)
            {
                Debug.LogError($"[GamePresenter] Failed to start new game: {result.ErrorMessage}");
            }
        }

        private void OnCellClicked(CellIndex index)
        {
            // Проверяем через State Machine
            if (!_stateMachine.CanPlayerMove())
            {
                Debug.Log($"Cannot move: current state is {_stateMachine.CurrentState}");
                return;
            }
            
            var state = _repository.GetCurrent();
            
            // Проверяем что сейчас ход игрока
            if (state.CurrentPlayer != Mark.Cross)
            {
                Debug.Log("Not player's turn");
                return;
            }

            // Проверяем что клетка пустая
            if (!state.Board.IsEmpty(index))
            {
                // Анимация ошибки
                if (_boardBuilder.CellViews.TryGetValue(index, out var cellView))
                {
                    cellView.PlayErrorShake();
                }
                return;
            }
            
            // Домен просчитывает мгновенно
            var result = _makeMove.Execute(index);
            
            if (!result.IsSuccess)
            {
                Debug.LogError($"[GamePresenter] Failed to make move: {result.ErrorMessage}");
                return;
            }
            
            // Уведомляем State Machine
            _stateMachine.OnPlayerMoved();
        }

        #endregion

        #region Domain Events

        private void OnGameStarted(GameStartedMessage evt)
        {
            Debug.Log("[Presenter] Game started");
            
            _animationQueue.Clear();
            RedrawBoardImmediate(evt.State);
            _stateMachine.OnGameStarted();
        }

        private void OnMoveMade(MoveMadeMessage msg)
        {
            Debug.Log($"[Presenter] Move made at {msg.LastMove.Row},{msg.LastMove.Column}");
            
            // Добавляем анимацию в очередь
            if (_boardBuilder.CellViews.TryGetValue(msg.LastMove, out var cellView))
            {
                var mark = msg.State.Board.GetMark(msg.LastMove);
                var animEvent = new MoveAnimationEvent(cellView, mark, msg.LastMove);
                _animationQueue.Enqueue(animEvent);
            }
        }

        private void OnGameFinished(GameFinishedMessage evt)
        {
            Debug.Log($"[Presenter] Game finished: {evt.State.Status}");
            _stateMachine.OnGameFinished();
        }

        #endregion

        #region State Machine Events

        /// <summary>
        /// Вызывается при любом изменении состояния
        /// </summary>
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
            
            // Блокируем клетки, но НЕ блокируем кнопку Reset
            SetCellsInteractionEnabled(false);
            
            // Кнопка Reset остается доступной!
            _statusView.ResetButton.interactable = true;
            
            // Можно добавить анимацию победы
            // PlayVictoryAnimation();
        }

        #endregion

        #region Animation Events

        private void OnAnimationStarted(IAnimationEvent evt)
        {
            Debug.Log($"[Presenter] Animation started: {evt.GetType().Name}");
            // Можно добавить звуки
        }

        private void OnAnimationCompleted(IAnimationEvent evt)
        {
            Debug.Log($"[Presenter] Animation completed: {evt.GetType().Name}");
            
            // Обновляем статус после каждой анимации
            var state = _repository.GetCurrent();
            UpdateStatusText(state, _stateMachine.CurrentState);
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

        /// <summary>
        /// Блокирует/разблокирует клетки игрового поля
        /// Кнопка Reset остается доступной!
        /// </summary>
        private void SetCellsInteractionEnabled(bool enabled)
        {
            foreach (var cellView in _boardBuilder.CellViews.Values)
            {
                cellView.Button.interactable = enabled;
            }

            Debug.Log($"[Presenter] Cells {(enabled ? "enabled" : "disabled")}");
        }

        private void UpdateStatusText(GameState state, GameFlowState flowState)
        {
            string statusText = _statusFormatter.GetStatusText(state, flowState);
            _statusView.StatusText.text = statusText;
        }

        #endregion
    }
}

