using System.Collections.Generic;
using DomoTroller2.ESFramework.Common.Base;

namespace DomoTroller2.ESFramework.Common.Interfaces
{
    public interface IEventStore
    {
        void Connect(string connectionstring, string user, string password, string certificateCommonName, bool useSsl = false, int reconnectAttempts = -1, int heartbeatInterval = 30, int heartbeatTimeout = 120);
        void SaveEvents(CompositeAggregateId aggregateId, IEnumerable<IEvent> events);
        void SaveEvent(IEvent @event, int expectedVersion);
        List<IEvent> GetAllEvents(CompositeAggregateId aggregateId);
        List<IEvent> GetAllEventsToEventIdInclusive(CompositeAggregateId aggregateId, string eventId);
        void CreatePersistentSubscription(string eventGroupName, string eventStreamName, Dictionary<string, string> createValues = null);
        IEventStreamSubscription GetEventStreamSubscription(string streamName);
        IEventStreamSubscription GetEventStreamPersistentSubscription(string eventGroupName, string eventStreamName);

    }
}
