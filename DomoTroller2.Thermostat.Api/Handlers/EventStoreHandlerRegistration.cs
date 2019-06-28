using DomoTroller2.Common.EventBus;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.Thermostat.Api.Handlers;

namespace DomoTroller2.Api.Handlers
{
    public static class EventStoreHandlerRegistration
    {
        public static void RegisterEventHandler(IEventStore eventStore)
        {

            var eventBus = EventBus.Instance;
            eventBus.RemoveHandlers();

            var thermostatHandlers = new EventStoreThermostatHandlers(eventStore);
            eventBus.RegisterEventHandler<ESEvents.Common.Events.Thermostat.Connected>(thermostatHandlers.Handler);
        }
    }
}
