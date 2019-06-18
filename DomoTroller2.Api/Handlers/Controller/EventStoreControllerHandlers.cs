using DomoTroller2.ESEvents.Common.Events.Controller;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Api.Handlers.Controller
{
    public class EventStoreControllerHandlers
    {
        public EventStoreControllerHandlers(IEventStore eventStore)
        { }

        public void Handler(Connected @event)
        {
            var controller = @event.AggregateGuid;
            var tenantId = @event.Metadata.TenantId;
        }
    }
}
