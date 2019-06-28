using DomoTroller2.Common.CommandBus;
using DomoTroller2.Thermostat.Api.Handlers;
using Thermostat.Common.Command;

namespace DomoTroller2.Thermostat.Api.Handlers
{
    public static class CommandHandlerRegistration
    {
        public static void RegisterCommandHandler()
        {
            var thermostatHandlers = new ThermostatCommandHandlers();

            var commandBus = CommandBus.Instance;
            commandBus.RemoveHandlers();
            commandBus.RegisterHandler<ConnectThermostat>(thermostatHandlers.Handle);
            commandBus.RegisterHandler<ChangeHeatSetpoint>(thermostatHandlers.Handle);
        }
    }
}
