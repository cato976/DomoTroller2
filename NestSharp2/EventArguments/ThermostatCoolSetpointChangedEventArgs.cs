using System;

namespace NestSharp2.EventArguments
{
    public class ThermostatCoolSetpointChangedEventArgs
    {
        public ThermostatCoolSetpointChangedEventArgs(Guid tenantId, string thermostatId, 
            Guid thermostatGuid, double? newCoolSetpoint)
        {
            TenantId = tenantId;
            ThermostatId = thermostatId;
            ThermostatGuid = thermostatGuid;
            NewCoolSetpoint = newCoolSetpoint;
        }

        public Guid TenantId { get; private set; }
        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public double? NewCoolSetpoint { get; private set; }
    }
}
