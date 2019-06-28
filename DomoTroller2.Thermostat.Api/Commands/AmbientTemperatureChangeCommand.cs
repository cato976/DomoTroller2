using System;

namespace DomoTroller2.Thermostat.Api.Commands
{
    public class AmbientTemperatureChangeCommand
    {
        public AmbientTemperatureChangeCommand(Guid tenantId, string thermostatId, 
            Guid thermostatGuid, 
            double? newAmbientTemperature)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewAmbientTemperature = newAmbientTemperature;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public double? NewAmbientTemperature { get; set; }
    }
}
