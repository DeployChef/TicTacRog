using System;

namespace TicTacRog.Infrastructure.Events
{
    public interface IMessageBus
    {
        void Publish<T>(T message);
        IDisposable Subscribe<T>(Action<T> handler);
    }
}
