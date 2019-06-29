using System;

namespace NestSharp2.EventArguments
{
    public class ThermostatHumidityChangedEventArgs : EventArgs
    {
        public ThermostatHumidityChangedEventArgs(Guid tenantId, string thermostatId,
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
