using Device.Common.Command;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.Api.Handlers.Device;
using Controller.Common.Command;
using DomoTroller2.Api.Handlers.Controller;

namespace DomoTroller2.Api.Handlers
{
    public static class CommandHandlerRegistration
    {
        public static void RegisterCommandHandler()
        {
            var deviceHandlers = new DeviceCommandHandlers();
            var controllerHandlers = new ControllerCommandHandlers();

            var commandBus = CommandBus.Instance;
            commandBus.RemoveHandlers();
            commandBus.RegisterHandler<TurnOn>(deviceHandlers.Handle);
            commandBus.RegisterHandler<ConnectToController>(controllerHandlers.Handle);
        }
    }
}
