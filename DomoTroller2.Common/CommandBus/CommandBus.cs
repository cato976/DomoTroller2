using System;
using System.Collections.Generic;

namespace DomoTroller2.Common.CommandBus
{
    public sealed class CommandBus : ICommandBus
    {
        private readonly Dictionary<Type, List<Action<ICommand>>> Routes = new Dictionary<Type, List<Action<ICommand>>>();

        private static readonly Lazy<CommandBus> Lazy = new Lazy<CommandBus>(() => new CommandBus());
        public static CommandBus Instance => Lazy.Value;

        private CommandBus()
        { }

        public void Execute<T>(T @event) where T : ICommand
        {
            List<Action<ICommand>> handlers;

            if (!Routes.TryGetValue(@event.GetType(), out handlers)) return;

            foreach (var handler in handlers)
            {
                var handler1 = handler;
                handler1(@event);
            }
        }

        public void RegisterHandler<T>(Action<T> handler) where T : ICommand
        {
            List<Action<ICommand>> handlers;

            if (!Routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<ICommand>>();
                Routes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }

        public void RemoveHandlers()
        {
            Routes.Clear();
        }
    }
}
