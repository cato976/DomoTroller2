using Device.Common.Command;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.Api.Handlers.Device;
using Controller.Common.Command;
using DomoTroller2.Api.Handlers.Controller;
using DomoTroller2.Api.Handlers.Thermostat;
using Thermostat.Common.Command;

namespace DomoTroller2.Api.Handlers
{
    public static class CommandHandlerRegistration
    {
        public static void RegisterCommandHandler()
        {
            var deviceHandlers = new DeviceCommandHandlers();
            var controllerHandlers = new ControllerCommandHandlers();
            var thermostatHandlers = new ThermostatCommandHandlers();

            var commandBus = CommandBus.Instance;
            commandBus.RemoveHandlers();
            commandBus.RegisterHandler<TurnOn>(deviceHandlers.Handle);
            commandBus.RegisterHandler<ConnectToController>(controllerHandlers.Handle);
            commandBus.RegisterHandler<ConnectThermostat>(thermostatHandlers.Handle);
        }
    }
}
