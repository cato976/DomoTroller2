using System;

namespace DomoTroller2.Thermostat.Api.Commands
{
    public class SystemModeChangeCommand
    {
        public SystemModeChangeCommand(Guid tenantId, string thermostatId, 
            Guid thermostatGuid, string newSystemMode)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewSystemMode = newSystemMode;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public string NewSystemMode { get; private set; }
    }
}
