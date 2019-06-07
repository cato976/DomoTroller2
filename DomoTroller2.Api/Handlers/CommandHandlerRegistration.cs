using Device.Common.Command;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.Api.Handlers.Device;

namespace DomoTroller2.Api.Handlers
{
    public static class CommandHandlerRegistration
    {
        public static void RegisterCommandHandler()
        {
            var deviceHandlers = new DeviceCommandHandlers();
            var commandBus = CommandBus.Instance;
            commandBus.RemoveHandlers();
            commandBus.RegisterHandler<TurnOn>(deviceHandlers.Handle);
        }
    }
}
