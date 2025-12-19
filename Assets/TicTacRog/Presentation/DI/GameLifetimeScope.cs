using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Bot;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation;
using TicTacRog.Presentation.Presenters;
using TicTacRog.Presentation.Views;
using TicTacRog.Presentation.Animation;
using TicTacRog.Presentation.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace TicTacRog.Presentation.DI
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private StatusView statusView;
        [SerializeField] private AnimationQueue animationQueue;

        [Header("Game Settings")]
        [SerializeField] private int boardSize = 3;
        [SerializeField] private Mark startingPlayer = Mark.Cross;

        [Header("Bot Settings")]
        [SerializeField] private float botThinkDelay = 0.5f;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateFields();
            
            builder.RegisterInstance(boardView);
            builder.RegisterInstance(statusView);
            builder.RegisterInstance(animationQueue);
            builder.RegisterInstance<MonoBehaviour>(animationQueue);

            builder.Register<InMemoryBoardRepository>(Lifetime.Singleton)
                .As<IBoardRepository>();
            
            builder.Register<Classic3X3RuleSet>(Lifetime.Singleton)
                .As<IGameRuleSet>();

            builder.Register<MessageBus>(Lifetime.Singleton)
                .As<IMessageBus>();
            
            builder.Register<GameEventsAdapter>(Lifetime.Singleton)
                .As<IGameEvents>();

            builder.Register<RandomBotPlayer>(Lifetime.Singleton)
                .As<IBotPlayer>();

            builder.Register<StartNewGameUseCase>(Lifetime.Singleton);
            builder.Register<MakeMoveUseCase>(Lifetime.Singleton);

            builder.Register<GameFlowStateMachine>(Lifetime.Singleton);

            builder.Register<BotController>(Lifetime.Singleton)
                .WithParameter("botThinkDelay", botThinkDelay);

            builder.Register<BoardBuilder>(Lifetime.Singleton);
            builder.Register<StatusTextFormatter>(Lifetime.Singleton);

            builder.Register<GamePresenter>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameEntryPoint>()
                .WithParameter("boardSize", boardSize)
                .WithParameter("startingPlayer", startingPlayer);
        }
        
        private void ValidateFields()
        {
            if (!boardView)
            {
                Debug.LogError("[GameLifetimeScope] BoardView is not assigned! " +
                    "Please drag BoardView from scene to the field in Inspector.");
                throw new System.NullReferenceException("BoardView is null");
            }
            
            if (!statusView)
            {
                Debug.LogError("[GameLifetimeScope] StatusView is not assigned! " +
                    "Please drag StatusView from scene to the field in Inspector.");
                throw new System.NullReferenceException("StatusView is null");
            }
            
            if (!animationQueue)
            {
                Debug.LogError("[GameLifetimeScope] AnimationQueue is not assigned! " +
                    "Please drag AnimationQueue GameObject to the field in Inspector.");
                throw new System.NullReferenceException("AnimationQueue is null");
            }
            
            Debug.Log("[GameLifetimeScope] ✓ All fields validated successfully");
        }
    }
}
