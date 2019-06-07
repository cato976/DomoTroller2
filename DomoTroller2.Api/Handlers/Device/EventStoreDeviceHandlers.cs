using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Api.Handlers.Device
{
    public class EventStoreDeviceHandlers
    {
        public EventStoreDeviceHandlers(IEventStore eventStore)
        { }

        public void Handler(TurnedOn @event)
        {
            var device = @event.AggregateGuid;
            var tenantId = @event.Metadata.TenantId;
        }

        public void Handler(SetLevel @event)
        {
            var device = @event.AggregateGuid;
            var tenantId = @event.Metadata.TenantId;
        }
    }
}
