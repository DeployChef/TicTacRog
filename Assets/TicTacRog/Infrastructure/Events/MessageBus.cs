using System;
using System.Collections.Generic;

namespace TicTacRog.Infrastructure.Events
{
    public sealed class MessageBus : IMessageBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<T>(T message)
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
            {
                foreach (var d in list.ToArray())
                    ((Action<T>)d)?.Invoke(message);
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (!_handlers.TryGetValue(t, out var list))
            {
                list = new List<Delegate>();
                _handlers[t] = list;
            }

            list.Add(handler);

            return new Subscription<T>(this, handler);
        }

        private sealed class Subscription<T> : IDisposable
        {
            private readonly MessageBus _bus;
            private readonly Action<T> _handler;
            private bool _disposed;

            public Subscription(MessageBus bus, Action<T> handler)
            {
                _bus = bus;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                if (_bus._handlers.TryGetValue(typeof(T), out var list))
                {
                    list.Remove(_handler);
                }
            }
        }
    }
}