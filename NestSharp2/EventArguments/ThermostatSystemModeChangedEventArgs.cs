using System;

namespace NestSharp2.EventArguments
{
    public class ThermostatSystemModeChangedEventArgs : EventArgs
    {
        public ThermostatSystemModeChangedEventArgs(Guid tenantId, string thermostatId,
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
