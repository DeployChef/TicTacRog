using System;
using System.Collections;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.StateMachine;

namespace TicTacRog.Presentation
{
    /// <summary>
    /// Контроллер бота - управляет КОГДА бот должен делать ход
    /// Слушает State Machine и реагирует на состояние BotThinking
    /// 
    /// Разделение ответственности:
    /// • State Machine → управляет состояниями UI
    /// • BotController → управляет ходами бота
    /// • IBotPlayer → решает КУДА ходить
    /// </summary>
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
            // Подписываемся на изменения состояний State Machine
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

        /// <summary>
        /// Реагируем на изменение состояния State Machine
        /// </summary>
        private void OnStateChanged(GameFlowState newState, GameFlowState oldState)
        {
            // Когда State Machine переходит в BotThinking - бот должен сходить
            if (newState == GameFlowState.BotThinking)
            {
                OnBotShouldThink();
            }
        }

        /// <summary>
        /// State Machine сигнализирует: пора боту подумать
        /// </summary>
        private void OnBotShouldThink()
        {
            Debug.Log("[BotController] Bot should think now");
            
            // Останавливаем предыдущую корутину если была
            if (_currentCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_currentCoroutine);
            }
            
            _currentCoroutine = _coroutineRunner.StartCoroutine(BotThinkCoroutine());
        }

        /// <summary>
        /// Корутина "думания" бота с задержкой
        /// </summary>
        private IEnumerator BotThinkCoroutine()
        {
            Debug.Log($"[BotController] Bot thinking for {BotThinkDelay}s...");
            
            // Задержка "думания" (визуальный эффект)
            yield return new WaitForSeconds(BotThinkDelay);

            _currentCoroutine = null;

            var state = _repository.GetCurrent();
            
            // Проверяем что игра всё ещё в процессе
            if (state.Status != GameStatus.InProgress)
            {
                Debug.LogWarning("[BotController] Game finished while bot was thinking");
                yield break;
            }

            // Проверяем что сейчас ход бота
            if (state.CurrentPlayer != Mark.Nought)
            {
                Debug.LogWarning("[BotController] Not bot's turn anymore");
                yield break;
            }

            // Бот делает ход (IBotPlayer решает КУДА)
            bool moveMade = _botPlayer.TryMakeMove(state);
            
            if (moveMade)
            {
                Debug.Log("[BotController] Bot made move successfully");
                
                // Переводим State Machine в состояние анимации хода бота
                // Это правильно: бот УЖЕ сделал ход, теперь будет анимация
                // (Событие уже добавлено в AnimationQueue в GamePresenter.OnMoveMade)
            }
            else
            {
                Debug.LogError("[BotController] Bot failed to make move!");
            }
        }
    }
}

