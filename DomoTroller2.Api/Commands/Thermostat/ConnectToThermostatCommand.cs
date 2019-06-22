using System;

namespace DomoTroller2.Api.Commands.Thermostat
{
    public class ConnectToThermostatCommand
    {
        public ConnectToThermostatCommand(string thermostatId, Guid thermostatGuid, double? temperature,
            double? heatSetpoint, double? coolSetpoint, string mode, string systemStatus, double? humidity = null)
        {
            ThermostatId = thermostatId;
            ThermostatAggregateId = thermostatGuid;
            Temperature = temperature;
            HeatSetpoint = heatSetpoint;
            CoolSetpoint = coolSetpoint;
            Humidity = humidity;
            Mode = mode;
            SystemStatus = systemStatus;
        }

        public string ThermostatId { get; set; }
        public Guid ThermostatAggregateId { get; set; }
        public double? Temperature { get; set; }
        public double? HeatSetpoint { get; set; }
        public double? CoolSetpoint { get; set; }
        public double? Humidity { get; set; }
        public string Mode { get; private set; }
        public string SystemStatus { get; private set; }
    }
}
