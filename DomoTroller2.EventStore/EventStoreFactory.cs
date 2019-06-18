using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.EventStore
{
    public static class EventStoreFactory
    {
        public static IEventStore CreateEventStore()
        {
            return new EventStoreImplementation();
        }
    }
}
