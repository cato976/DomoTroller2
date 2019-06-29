using System;

namespace DomoTroller2.Thermostat.Api.Commands
{
    public class HumidityChangeCommand
    {
        public HumidityChangeCommand(Guid tenantId, string thermostatId, 
            Guid thermostatGuid, double? newHumidity)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewHumidity = newHumidity;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public double? NewHumidity { get; private set; }
    }
}
