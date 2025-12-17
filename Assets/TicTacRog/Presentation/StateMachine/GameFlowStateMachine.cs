using System;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Animation;

namespace TicTacRog.Presentation.StateMachine
{
    /// <summary>
    /// State Machine для управления игровым потоком UI
    /// Отвечает ТОЛЬКО за состояния: WaitingForPlayer, AnimatingMove, BotThinking, GameFinished
    /// НЕ управляет ботом напрямую - это делает BotController
    /// </summary>
    public sealed class GameFlowStateMachine : IDisposable
    {
        private readonly IBoardRepository _repository;
        private readonly AnimationQueue _animationQueue;

        private GameFlowState _currentState = GameFlowState.WaitingForPlayerInput;
        private GameFlowState _previousState;

        public bool EnableDebugLogs { get; set; } = true;

        public GameFlowState CurrentState => _currentState;
        public GameFlowState PreviousState => _previousState;
        
        /// <summary>
        /// Событие изменения состояния (newState, oldState)
        /// </summary>
        public event Action<GameFlowState, GameFlowState> OnStateChanged;
        
        /// <summary>
        /// События входа в конкретные состояния
        /// </summary>
        public event Action OnEnteredWaitingForPlayer;
        public event Action OnEnteredBotThinking;
        public event Action OnEnteredGameFinished;

        public GameFlowStateMachine(
            IBoardRepository repository,
            AnimationQueue animationQueue)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _animationQueue = animationQueue ?? throw new ArgumentNullException(nameof(animationQueue));
        }

        public void Initialize()
        {
            _animationQueue.OnQueueCompleted += OnAnimationCompleted;
            TransitionTo(GameFlowState.WaitingForPlayerInput);
        }

        public void Dispose()
        {
            _animationQueue.OnQueueCompleted -= OnAnimationCompleted;
        }

        /// <summary>
        /// Можно ли сейчас делать ход игроку
        /// </summary>
        public bool CanPlayerMove()
        {
            return _currentState == GameFlowState.WaitingForPlayerInput;
        }

        /// <summary>
        /// Игрок сделал ход
        /// </summary>
        public void OnPlayerMoved()
        {
            if (_currentState != GameFlowState.WaitingForPlayerInput)
            {
                LogWarning($"OnPlayerMoved called in wrong state: {_currentState}");
                return;
            }

            TransitionTo(GameFlowState.AnimatingPlayerMove);
        }

        /// <summary>
        /// Новая игра началась
        /// </summary>
        public void OnGameStarted()
        {
            TransitionTo(GameFlowState.WaitingForPlayerInput);
        }

        /// <summary>
        /// Игра завершена
        /// </summary>
        public void OnGameFinished()
        {
            // Защита от повторного вызова
            if (_currentState == GameFlowState.GameFinished)
            {
                Log("OnGameFinished called but already in GameFinished state");
                return;
            }
            
            TransitionTo(GameFlowState.GameFinished);
        }

        /// <summary>
        /// Вызывается когда очередь анимаций завершена
        /// ВАЖНО: Это означает что ВСЕ анимации в очереди закончились!
        /// </summary>
        private void OnAnimationCompleted()
        {
            var state = _repository.GetCurrent();

            Log($"Animation completed. Game status: {state.Status}, Current flow state: {_currentState}");

            // Игра завершена?
            if (state.Status != GameStatus.InProgress)
            {
                // Если уже в GameFinished, не пытаться перейти туда снова
                if (_currentState != GameFlowState.GameFinished)
                {
                    TransitionTo(GameFlowState.GameFinished);
                }
                else
                {
                    Log("Already in GameFinished state, skipping transition");
                }
                return;
            }

            // Определяем следующее состояние на основе ТЕКУЩЕГО состояния
            // Когда очередь завершена, мы знаем что анимация для текущего состояния закончилась
            switch (_currentState)
            {
                case GameFlowState.AnimatingPlayerMove:
                    HandlePlayerMoveAnimationCompleted(state);
                    break;
                
                case GameFlowState.BotThinking:
                    // Бот сделал ход и анимация ЗАКОНЧИЛАСЬ (OnQueueCompleted)
                    // Обрабатываем как завершение хода бота
                    Log("BotThinking animation completed → checking next state");
                    HandleBotMoveAnimationCompleted(state);
                    break;

                case GameFlowState.AnimatingBotMove:
                    HandleBotMoveAnimationCompleted(state);
                    break;
            }
        }

        private void HandlePlayerMoveAnimationCompleted(GameState state)
        {
            // После анимации хода игрока проверяем, чей ход
            if (state.CurrentPlayer == Mark.Nought)
            {
                // Ход бота - переходим в состояние BotThinking
                // BotController услышит это событие и вызовет бота
                TransitionTo(GameFlowState.BotThinking);
            }
            else
            {
                // Снова ход игрока (например, в режиме PvP)
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
        }

        private void HandleBotMoveAnimationCompleted(GameState state)
        {
            // После анимации хода бота → ход игрока
            if (state.CurrentPlayer == Mark.Cross)
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
            else
            {
                // Если вдруг опять ход бота (в режиме Bot vs Bot)
                TransitionTo(GameFlowState.BotThinking);
            }
        }

        private void TransitionTo(GameFlowState newState)
        {
            if (_currentState == newState)
            {
                LogWarning($"Attempted transition to same state: {newState}");
                return;
            }

            _previousState = _currentState;
            _currentState = newState;

            Log($"[STATE] {_previousState} → {_currentState}");
            
            // Вызываем общее событие
            OnStateChanged?.Invoke(_currentState, _previousState);

            // Вызываем специфичные события
            InvokeStateSpecificEvent(newState);
        }

        private void InvokeStateSpecificEvent(GameFlowState state)
        {
            switch (state)
            {
                case GameFlowState.WaitingForPlayerInput:
                    OnEnteredWaitingForPlayer?.Invoke();
                    break;
                case GameFlowState.BotThinking:
                    OnEnteredBotThinking?.Invoke();
                    break;
                case GameFlowState.GameFinished:
                    OnEnteredGameFinished?.Invoke();
                    break;
            }
        }

        private void Log(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[GameFlowSM] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.LogWarning($"[GameFlowSM] {message}");
            }
        }
    }
}

