using System;
using System.Collections;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.StateMachine;

namespace TicTacRog.Presentation
{
    public sealed class BotController : IDisposable
    {
        private readonly IBotPlayer _botPlayer;
        private readonly IBoardRepository _repository;
        private readonly GameFlowStateMachine _stateMachine;
        private readonly MonoBehaviour _coroutineRunner;

        private Coroutine _currentCoroutine;

        public float BotThinkDelay { get; set; } = 0.5f;

        public BotController(
            IBotPlayer botPlayer,
            IBoardRepository repository,
            GameFlowStateMachine stateMachine,
            MonoBehaviour coroutineRunner)
        {
            _botPlayer = botPlayer ?? throw new ArgumentNullException(nameof(botPlayer));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
        }

        public void Initialize()
        {
            _stateMachine.OnStateChanged += OnStateChanged;
            
            Debug.Log("[BotController] Initialized");
        }

        public void Dispose()
        {
            _stateMachine.OnStateChanged -= OnStateChanged;
            
            if (_currentCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }
            
            Debug.Log("[BotController] Disposed");
        }

        private void OnStateChanged(GameFlowState newState, GameFlowState oldState)
        {
            if (newState == GameFlowState.BotThinking)
            {
                OnBotShouldThink();
            }
        }

        private void OnBotShouldThink()
        {
            Debug.Log("[BotController] Bot should think now");
            
            if (_currentCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_currentCoroutine);
            }
            
            _currentCoroutine = _coroutineRunner.StartCoroutine(BotThinkCoroutine());
        }

        private IEnumerator BotThinkCoroutine()
        {
            Debug.Log($"[BotController] Bot thinking for {BotThinkDelay}s...");
            
            yield return new WaitForSeconds(BotThinkDelay);

            _currentCoroutine = null;

            var state = _repository.GetCurrent();
            
            if (state.Status != GameStatus.InProgress)
            {
                Debug.LogWarning("[BotController] Game finished while bot was thinking");
                yield break;
            }

            if (state.CurrentPlayerType != SymbolType.Nought)
            {
                Debug.LogWarning("[BotController] Not bot's turn anymore");
                yield break;
            }

            bool moveMade = _botPlayer.TryMakeMove(state);
            
            if (moveMade)
            {
                Debug.Log("[BotController] Bot made move successfully");
            }
            else
            {
                Debug.LogError("[BotController] Bot failed to make move!");
            }
        }
    }
}

