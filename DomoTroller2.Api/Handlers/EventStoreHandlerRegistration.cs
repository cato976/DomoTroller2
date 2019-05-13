using DomoTroller2.Common.EventBus;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Api.Handlers
{
    public static class EventStoreHandlerRegistration
    {
        public static void RegisterEventHandler(IEventStore eventStore)
        {
            var deviceHandlers = new EventStoreDeviceHandlers(eventStore);
            var eventBus = EventBus.Instance;
            eventBus.RemoveHandlers();
            eventBus.RegisterEventHandler<TurnedOn>(deviceHandlers.Handler);
            eventBus.RegisterEventHandler<SetLevel>(deviceHandlers.Handler);

        }
    }
}
