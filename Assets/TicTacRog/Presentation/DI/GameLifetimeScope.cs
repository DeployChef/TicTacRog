using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Repositories;
using TicTacRog.Presentation.Presenters;
using TicTacRog.Presentation.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TicTacRog.Presentation.DI
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private StatusView _statusView;
        [SerializeField] private int _boardSize = 3;
        [SerializeField] private Mark _startingPlayer = Mark.Cross;

        protected override void Configure(IContainerBuilder builder)
        {
            // Вьюхи (как инстансы сцены)
            builder.RegisterInstance(_boardView);
            builder.RegisterInstance(_statusView);

            // Доменные сервисы
            builder.Register<InMemoryBoardRepository>(Lifetime.Singleton)
                .As<IBoardRepository>();
            builder.Register<Classic3x3RuleSet>(Lifetime.Singleton)
                .As<IGameRuleSet>();

            // Шина и события
            builder.Register<EventBus>(Lifetime.Singleton);
            builder.Register<GameEventsAdapter>(Lifetime.Singleton)
                .As<IGameEvents>();

            // Use cases
            builder.Register<StartNewGameUseCase>(Lifetime.Singleton);
            builder.Register<MakeMoveUseCase>(Lifetime.Singleton);

            // Presenter
            builder.Register<GamePresenter>(Lifetime.Singleton);

            // Entry point
            builder.RegisterEntryPoint<GameEntryPoint>()
                .WithParameter(_boardSize)
                .WithParameter(_startingPlayer);
        }
    }
}