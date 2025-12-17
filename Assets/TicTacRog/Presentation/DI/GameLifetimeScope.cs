using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Presenters;
using TicTacRog.Presentation.Views;
using TicTacRog.Presentation.Animation;
using TicTacRog.Presentation.StateMachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TicTacRog.Presentation.DI
{
    /// <summary>
    /// DI контейнер для композиции всех зависимостей игры
    /// ФИНАЛЬНАЯ ВЕРСИЯ с State Machine + Animation Queue
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Views")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private StatusView _statusView;
        [SerializeField] private AnimationQueue _animationQueue;

        [Header("Game Settings")]
        [SerializeField] private int _boardSize = 3;
        [SerializeField] private Mark _startingPlayer = Mark.Cross;

        [Header("Bot Settings")]
        [SerializeField] private float _botThinkDelay = 0.5f;

        protected override void Configure(IContainerBuilder builder)
        {
            // Валидация (проверяем что все поля заполнены)
            ValidateFields();
            
            // Views (как экземпляры из сцены)
            builder.RegisterInstance(_boardView);
            builder.RegisterInstance(_statusView);
            builder.RegisterInstance(_animationQueue);
            
            // AnimationQueue это MonoBehaviour - регистрируем его также как MonoBehaviour
            // для использования в GameFlowStateMachine (для корутин)
            builder.RegisterInstance<MonoBehaviour>(_animationQueue);

            // Domain (бизнес-логика)
            builder.Register<InMemoryBoardRepository>(Lifetime.Singleton)
                .As<IBoardRepository>();
            
            builder.Register<Classic3x3RuleSet>(Lifetime.Singleton)
                .As<IGameRuleSet>();

            // Infrastructure (события и реализации)
            builder.Register<MessageBus>(Lifetime.Singleton)
                .As<IMessageBus>();
            
            builder.Register<GameEventsAdapter>(Lifetime.Singleton)
                .As<IGameEvents>();

            // Bot
            builder.Register<RandomBotPlayer>(Lifetime.Singleton)
                .As<IBotPlayer>();

            // Use Cases
            builder.Register<StartNewGameUseCase>(Lifetime.Singleton);
            builder.Register<MakeMoveUseCase>(Lifetime.Singleton);

            // State Machine (НОВОЕ!)
            builder.Register<GameFlowStateMachine>(Lifetime.Singleton)
                .WithParameter("botThinkDelay", _botThinkDelay);

            // Presenter (ОБНОВЛЕН для State Machine)
            builder.Register<GamePresenter>(Lifetime.Singleton);

            // Entry Point
            builder.RegisterEntryPoint<GameEntryPoint>()
                .WithParameter("boardSize", _boardSize)
                .WithParameter("startingPlayer", _startingPlayer);
        }
        
        /// <summary>
        /// Проверяем что все обязательные поля заполнены в Inspector
        /// </summary>
        private void ValidateFields()
        {
            if (_boardView == null)
            {
                Debug.LogError("[GameLifetimeScope] BoardView is not assigned! " +
                    "Please drag BoardView from scene to the field in Inspector.");
                throw new System.NullReferenceException("BoardView is null");
            }
            
            if (_statusView == null)
            {
                Debug.LogError("[GameLifetimeScope] StatusView is not assigned! " +
                    "Please drag StatusView from scene to the field in Inspector.");
                throw new System.NullReferenceException("StatusView is null");
            }
            
            if (_animationQueue == null)
            {
                Debug.LogError("[GameLifetimeScope] AnimationQueue is not assigned! " +
                    "Please drag AnimationQueue GameObject to the field in Inspector.");
                throw new System.NullReferenceException("AnimationQueue is null");
            }
            
            Debug.Log("[GameLifetimeScope] ✓ All fields validated successfully");
        }
    }
}
