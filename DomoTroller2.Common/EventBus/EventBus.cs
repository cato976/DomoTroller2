using System;
using System.Collections.Generic;

using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Common.EventBus
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Action<IEvent>>> Routes = new Dictionary<Type, List<Action<IEvent>>>();

        private static readonly Lazy<EventBus> Lazy = new Lazy<EventBus>(() => new EventBus());

        public static EventBus Instance => Lazy.Value;

        private EventBus() { }

        public void RemoveHandlers()
        {
            Routes.Clear();
        }

        public void RegisterEventHandler<T>(Action<T> handler) where T : IEvent
        {
            List<Action<IEvent>> handlers;

            if (!Routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<IEvent>>();
                Routes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }

        public void Execute<T>(T @event) where T : IEvent
        {
            List<Action<IEvent>> handlers;

            if (!Routes.TryGetValue(@event.GetType(), out handlers)) return;

            foreach (var handler in handlers)
            {
                var handler1 = handler;
                handler1(@event);
            }
        }
    }
}
