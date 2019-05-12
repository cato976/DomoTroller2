using System;

namespace DomoTroller2.Common.CommandBus
{
    public interface ICommandBus
    {
        void RemoveHandlers();
        void RegisterHandler<T>(Action<T> handler) where T : ICommand;
        void Execute<T>(T @event) where T : ICommand;
    }
}
