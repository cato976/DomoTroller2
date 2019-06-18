using System;

namespace DomoTroller2.Api.Commands.Thermostat
{
    public class ConnectToThermostatCommand
    {
        public ConnectToThermostatCommand(Guid thermostatId)
        {
            ThermostatId = thermostatId;
        }

        public Guid ThermostatId { get; set; }
    }
}
