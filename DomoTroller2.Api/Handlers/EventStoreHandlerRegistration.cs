using DomoTroller2.Common.EventBus;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESEvents.Common.Events.Unit;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.Api.Handlers.Device;
using DomoTroller2.Api.Handlers.Unit;
using DomoTroller2.Api.Handlers.Controller;
using DomoTroller2.ESEvents.Common.Events.Controller;

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

            var unitHandlers = new EventStoreUnitHandlers(eventStore);
            eventBus.RegisterEventHandler<DoorOpened>(unitHandlers.Handler);
            
            var controllerHandlers = new EventStoreControllerHandlers(eventStore);
            eventBus.RegisterEventHandler<Connected>(controllerHandlers.Handler);
        }
    }
}
