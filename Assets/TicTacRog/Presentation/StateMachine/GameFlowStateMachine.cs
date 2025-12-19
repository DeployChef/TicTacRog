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

        public void OnGameStarted()
        {
            TransitionTo(GameFlowState.WaitingForPlayerInput);
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
            if (state.CurrentPlayer == Mark.Nought)
            {
                TransitionTo(GameFlowState.BotThinking);
            }
            else
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
        }

        private void HandleBotMoveAnimationCompleted(GameState state)
        {
            if (state.CurrentPlayer == Mark.Cross)
            {
                TransitionTo(GameFlowState.WaitingForPlayerInput);
            }
            else
            {
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

