using System;

namespace DomoTroller2.Thermostat.Api.Commands
{
    public class CoolSetpointChangeCommand
    {
        public CoolSetpointChangeCommand(Guid tenantId, string thermostatId, Guid thermostatGuid, 
            double? newCoolSetpoint)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewCoolSetpoint = newCoolSetpoint;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public double? NewCoolSetpoint { get; set; }
    }
}
