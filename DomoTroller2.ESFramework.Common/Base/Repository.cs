using DomoTroller2.ESFramework.Common.Interfaces;
using System;
using System.Diagnostics;

namespace DomoTroller2.ESFramework.Common.Base
{
    public class Repository<T> : IRepository<T> where T : Aggregate, new()
    {
        public Repository(IEventStore storage)
        {
            Storage = storage;
        }

        private readonly IEventStore Storage;

        public T GetById(CompositeAggregateId aggregateId)
        {
            var obj = new T();
            try
            {
                var e = Storage.GetAllEvents(aggregateId);
                obj.Replay(e);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error: {ex}");
            }
            return obj;
        }
    }
}
