using System;
using TicTacRog.Core.UseCases;
using TicTacRog.Infrastructure.Events;
using TicTacRog.Infrastructure.Events.Messages;

namespace TicTacRog.Infrastructure
{
    public sealed class BotListener : IDisposable
    {
        private readonly IMessageBus _bus;
        private readonly IBotPlayer _botPlayer;
        private IDisposable _moveSub;

        public BotListener(IMessageBus bus, IBotPlayer botPlayer)
        {
            _bus = bus;
            _botPlayer = botPlayer;
        }

        public void Initialize()
        {
            _moveSub = _bus.Subscribe<MoveMadeMessage>(OnMoveMade);
        }

        private void OnMoveMade(MoveMadeMessage msg)
        {
            _botPlayer.TryMakeMove(msg.State);
        }

        public void Dispose()
        {
            _moveSub?.Dispose();
        }
    }
}
