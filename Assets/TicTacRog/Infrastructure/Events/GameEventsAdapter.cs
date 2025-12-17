using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;

namespace TicTacRog.Infrastructure.Events
{
    public sealed class GameEventsAdapter : IGameEvents
    {
        private readonly EventBus _bus;

        public GameEventsAdapter(EventBus bus) => _bus = bus;

        public void OnGameStarted(GameState state) =>
            _bus.Publish(new GameStartedEvent(state));

        public void OnMoveMade(GameState state, CellIndex lastMove) =>
            _bus.Publish(new MoveMadeEvent(state, lastMove));

        public void OnGameFinished(GameState state) =>
            _bus.Publish(new GameFinishedEvent(state));
    }
}
