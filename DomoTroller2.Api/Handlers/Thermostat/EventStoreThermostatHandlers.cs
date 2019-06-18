using DomoTroller2.ESEvents.Common.Events.Thermostat;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Api.Handlers.Thermostat
{
    public class EventStoreThermostatHandlers
    {
        public EventStoreThermostatHandlers(IEventStore eventStore)
        { }

        public void Handler(Connected @event)
        {
            var controller = @event.AggregateGuid;
            var tenantId = @event.Metadata.TenantId;
        }
    }
}
