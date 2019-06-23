using System;

namespace DomoTroller2.Api.Commands.Thermostat
{
    public class ConnectToThermostatCommand
    {
        public ConnectToThermostatCommand(Guid tenantId, string thermostatId, Guid thermostatGuid, double? temperature,
            double? heatSetpoint, double? coolSetpoint, string mode, string systemStatus, double? humidity = null)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatAggregateId = thermostatGuid;
            Temperature = temperature;
            HeatSetpoint = heatSetpoint;
            CoolSetpoint = coolSetpoint;
            Humidity = humidity;
            Mode = mode;
            SystemStatus = systemStatus;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatAggregateId { get; private set; }
        public double? Temperature { get; private set; }
        public double? HeatSetpoint { get; private set; }
        public double? CoolSetpoint { get; private set; }
        public double? Humidity { get; private set; }
        public string Mode { get; private set; }
        public string SystemStatus { get; private set; }
    }
}
