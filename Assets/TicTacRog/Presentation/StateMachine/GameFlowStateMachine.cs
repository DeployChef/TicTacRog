using System;
using System.Collections;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Animation;

namespace TicTacRog.Presentation.StateMachine
{
    /// <summary>
    /// State Machine для управления игровым потоком
    /// Управляет переходами между состояниями UI в зависимости от игровой логики
    /// </summary>
    public sealed class GameFlowStateMachine : IDisposable
    {
        private readonly IBotPlayer _botPlayer;
        private readonly IBoardRepository _repository;
        private readonly AnimationQueue _animationQueue;
        private readonly MonoBehaviour _coroutineRunner;

        private GameFlowState _currentState = GameFlowState.WaitingForPlayerInput;
        private GameFlowState _previousState;
        private Coroutine _currentCoroutine;

        public float BotThinkDelay { get; set; } = 0.5f;
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
            IBotPlayer botPlayer,
            IBoardRepository repository,
            AnimationQueue animationQueue,
            MonoBehaviour coroutineRunner)
        {
            _botPlayer = botPlayer ?? throw new ArgumentNullException(nameof(botPlayer));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _animationQueue = animationQueue ?? throw new ArgumentNullException(nameof(animationQueue));
            _coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
        }

        public void Initialize()
        {
            _animationQueue.OnQueueCompleted += OnAnimationCompleted;
            TransitionTo(GameFlowState.WaitingForPlayerInput);
        }

        public void Dispose()
        {
            _animationQueue.OnQueueCompleted -= OnAnimationCompleted;
            
            if (_currentCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }
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
            // Останавливаем текущую корутину если есть
            if (_currentCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            TransitionTo(GameFlowState.WaitingForPlayerInput);
        }

        /// <summary>
        /// Игра завершена
        /// </summary>
        public void OnGameFinished()
        {
            TransitionTo(GameFlowState.GameFinished);
        }

        /// <summary>
        /// Вызывается когда очередь анимаций завершена
        /// </summary>
        private void OnAnimationCompleted()
        {
            var state = _repository.GetCurrent();

            Log($"Animation completed. Game status: {state.Status}, Current flow state: {_currentState}");

            // Игра завершена?
            if (state.Status != GameStatus.InProgress)
            {
                TransitionTo(GameFlowState.GameFinished);
                return;
            }

            // Определяем следующее состояние
            switch (_currentState)
            {
                case GameFlowState.AnimatingPlayerMove:
                    HandlePlayerMoveAnimationCompleted(state);
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
                // Ход бота
                TransitionTo(GameFlowState.BotThinking);
                _currentCoroutine = _coroutineRunner.StartCoroutine(BotThinkCoroutine());
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
                _currentCoroutine = _coroutineRunner.StartCoroutine(BotThinkCoroutine());
            }
        }

        private IEnumerator BotThinkCoroutine()
        {
            Log($"Bot thinking for {BotThinkDelay}s...");
            
            yield return new WaitForSeconds(BotThinkDelay);

            _currentCoroutine = null;

            var state = _repository.GetCurrent();
            
            // Проверяем что игра всё ещё в процессе
            if (state.Status != GameStatus.InProgress)
            {
                Log("Game finished while bot was thinking");
                TransitionTo(GameFlowState.GameFinished);
                yield break;
            }

            // Бот делает ход
            bool moveMade = _botPlayer.TryMakeMove(state);
            
            if (moveMade)
            {
                Log("Bot made move, transitioning to AnimatingBotMove");
                TransitionTo(GameFlowState.AnimatingBotMove);
            }
            else
            {
                LogWarning("Bot failed to make move");
                TransitionTo(GameFlowState.WaitingForPlayerInput);
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

