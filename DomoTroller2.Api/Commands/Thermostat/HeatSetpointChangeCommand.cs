using System;

namespace DomoTroller2.Api.Commands.Thermostat
{
    public class HeatSetpointChangeCommand
    {
        public HeatSetpointChangeCommand(Guid tenantId, string thermostatId, Guid thermostatGuid, 
            double? newHeatSetpoint)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewHeatSetpoint = newHeatSetpoint;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public double? NewHeatSetpoint { get; set; }
    }
}
