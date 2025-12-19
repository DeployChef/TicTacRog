using System.Collections.Generic;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events.Messages;

namespace TicTacRog.Infrastructure.Events
{
    public sealed class GameEventsAdapter : IGameEvents
    {
        private readonly IMessageBus _bus;

        public GameEventsAdapter(IMessageBus bus)
        {
            _bus = bus;
        }

        public void OnGameStarted(GameState state)
            => _bus.Publish(new GameStartedMessage(state));

        public void OnMoveMade(GameState state, CellIndex lastMove)
            => _bus.Publish(new MoveMadeMessage(state, lastMove));

        public void OnGameFinished(GameState state, IReadOnlyList<CellIndex> winningCells)
            => _bus.Publish(new GameFinishedMessage(state, winningCells));
    }
}