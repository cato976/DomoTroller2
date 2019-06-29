using System;

namespace DomoTroller2.Thermostat.Api.Commands
{
    public class SystemStatusChangeCommand
    {
        public SystemStatusChangeCommand(Guid tenantId, string thermostatId, 
            Guid thermostatGuid, string newSystemStatus)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewSystemStatus = newSystemStatus;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public string NewSystemStatus { get; private set; }
    }
}
