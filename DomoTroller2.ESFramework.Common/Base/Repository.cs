using DomoTroller2.ESFramework.Common.Interfaces;
using System;
using System.Diagnostics;

namespace DomoTroller2.ESFramework.Common.Base
{
    public class Repository<T> : IRepository<T> where T : Aggregate, new()
    {
        private readonly IEventStore _storage;

        public Repository(IEventStore storage)
        {
            _storage = storage;
        }

        public T GetById(CompositeAggregateId aggregateId)
        {
            var obj = new T();
            try
            {
                var e = _storage.GetAllEvents(aggregateId);
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
