using System;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Animation;

namespace TicTacRog.Presentation.StateMachine
{
    public sealed class GameFlowStateMachine : IDisposable
    {
        private readonly IBoardRepository _repository;
        private readonly AnimationQueue _animationQueue;
        private readonly IGameEvents _gameEvents;

        private GameFlowState _currentState = GameFlowState.WaitingForPlayerInput;
        private GameFlowState _previousState;

        public bool EnableDebugLogs { get; set; } = true;

        public GameFlowState CurrentState => _currentState;
        public GameFlowState PreviousState => _previousState;
        
        public event Action<GameFlowState, GameFlowState> OnStateChanged;
        
        public event Action OnEnteredWaitingForPlayer;
        public event Action OnEnteredBotThinking;
        public event Action OnEnteredGameFinished;

        public GameFlowStateMachine(
            IBoardRepository repository,
            AnimationQueue animationQueue,
            IGameEvents gameEvents)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _animationQueue = animationQueue ?? throw new ArgumentNullException(nameof(animationQueue));
            _gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(gameEvents));
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

        public bool CanPlayerMove()
        {
            return _currentState == GameFlowState.WaitingForPlayerInput;
        }

        public void OnPlayerMoved()
        {
            if (_currentState != GameFlowState.WaitingForPlayerInput)
            {
                LogWarning($"OnPlayerMoved called in wrong state: {_currentState}");
                return;
            }

            TransitionTo(GameFlowState.AnimatingPlayerMove);
        }

        public void OnBotCannotMove()
        {
            // Если бот не может сделать ход, пропускаем ход и переходим к следующему состоянию
            var state = _repository.GetCurrent();
            if (state == null || state.Status != GameStatus.InProgress)
                return;

            Log("Bot cannot make move, skipping turn");
            SkipTurnsUntilSomeoneCanMove(state);
            
            if (state.Status != GameStatus.InProgress)
            {
                TransitionTo(GameFlowState.GameFinished);
                return;
            }
            
            if (state.CurrentPlayerType == SymbolType.Cross)
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
            else
            {
                TransitionTo(GameFlowState.BotThinking);
            }
        }

        public void OnGameStarted()
        {
            var state = _repository.GetCurrent();
            
            // Пропускаем ходы, пока кто-то не сможет сделать ход
            SkipTurnsUntilSomeoneCanMove(state);
            
            if (state.Status != GameStatus.InProgress)
            {
                TransitionTo(GameFlowState.GameFinished);
                return;
            }
            
            if (state.CurrentPlayerType == SymbolType.Cross)
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
            else
            {
                TransitionTo(GameFlowState.BotThinking);
            }
        }

        public void OnGameFinished()
        {
            if (_currentState == GameFlowState.GameFinished)
            {
                Log("OnGameFinished called but already in GameFinished state");
                return;
            }
            
            TransitionTo(GameFlowState.GameFinished);
        }

        private void OnAnimationCompleted()
        {
            var state = _repository.GetCurrent();

            Log($"Animation completed. Game status: {state.Status}, Current flow state: {_currentState}");

            if (state.Status != GameStatus.InProgress)
            {
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

            switch (_currentState)
            {
                case GameFlowState.AnimatingPlayerMove:
                    HandlePlayerMoveAnimationCompleted(state);
                    break;
                
                case GameFlowState.BotThinking:
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
            // Пропускаем ходы, пока кто-то не сможет сделать ход
            SkipTurnsUntilSomeoneCanMove(state);
            
            if (state.Status != GameStatus.InProgress)
            {
                TransitionTo(GameFlowState.GameFinished);
                return;
            }
            
            if (state.CurrentPlayerType == SymbolType.Cross)
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
            else
            {
                TransitionTo(GameFlowState.BotThinking);
            }
        }

        private void HandleBotMoveAnimationCompleted(GameState state)
        {
            // Пропускаем ходы, пока кто-то не сможет сделать ход
            SkipTurnsUntilSomeoneCanMove(state);
            
            if (state.Status != GameStatus.InProgress)
            {
                TransitionTo(GameFlowState.GameFinished);
                return;
            }
            
            if (state.CurrentPlayerType == SymbolType.Cross)
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
            else
            {
                TransitionTo(GameFlowState.BotThinking);
            }
        }

        private bool CanCurrentPlayerMakeMove(GameState state)
        {
            var currentHand = state.GetCurrentHand();
            var currentDeck = state.GetCurrentDeck();
            
            // Текущий игрок может сделать ход, если есть карты в руке или в деке
            return !currentHand.IsEmpty || !currentDeck.IsEmpty;
        }


        private void SkipTurnsUntilSomeoneCanMove(GameState state)
        {
            int maxIterations = 10; // Защита от бесконечного цикла
            int iterations = 0;
            
            while (!CanCurrentPlayerMakeMove(state) && iterations < maxIterations)
            {
                Log($"Current player ({state.CurrentPlayerType}) has no cards, skipping turn");
                state.SwitchPlayer();
                iterations++;
            }
            
            if (iterations >= maxIterations)
            {
                LogWarning("Reached max iterations in SkipTurnsUntilSomeoneCanMove - possible infinite loop");
            }
            
            // Если после всех переключений никто не может сделать ход - проверяем ничью
            if (!CanCurrentPlayerMakeMove(state))
            {
                // Проверяем, есть ли свободные ячейки на доске
                bool hasEmptyCells = false;
                for (int row = 0; row < state.Board.Size; row++)
                {
                    for (int col = 0; col < state.Board.Size; col++)
                    {
                        if (state.Board.IsEmpty(new CellIndex(row, col)))
                        {
                            hasEmptyCells = true;
                            break;
                        }
                    }
                    if (hasEmptyCells) break;
                }
                
                if (!hasEmptyCells)
                {
                    // Доска заполнена - ничья
                    Log("Board is full and both players have no cards - declaring draw");
                    state.SetStatus(GameStatus.Draw);
                    _repository.Save(state);
                    _gameEvents.OnGameFinished(state, System.Array.Empty<CellIndex>());
                }
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
            
            OnStateChanged?.Invoke(_currentState, _previousState);

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

