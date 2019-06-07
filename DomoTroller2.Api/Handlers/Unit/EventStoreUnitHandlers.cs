using DomoTroller2.ESEvents.Common.Events.Unit;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Api.Handlers.Unit
{
    public class EventStoreUnitHandlers
    {
        public EventStoreUnitHandlers(IEventStore eventStore)
        { }

        public void Handler(DoorOpened @event)
        {
            var device = @event.AggregateGuid;
            var tenantId = @event.Metadata.TenantId;
        }
    }
}
