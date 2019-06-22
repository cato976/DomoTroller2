using System;

namespace DomoTrollerShare2.EventArguments
{
    public class ThermostatConnectedEventArgs : EventArgs
    {
        public ThermostatConnectedEventArgs(string thermostatId, Guid theremostatGuid, double? temperature, 
            double? heatSetpoint, double? coolSetpoint, string mode, string systemStatus, double? humidity = null)
        {
            ThermostatId = thermostatId;
            ThermostatGuid = theremostatGuid;
            Temperature = temperature;
            HeatSetpoint = heatSetpoint;
            CoolSetpoint = coolSetpoint;
            Humidity = humidity;
            Mode = mode;
            SystemStatus = systemStatus;
        }

        public string ThermostatId { get; private set; }
        public Guid ThermostatGuid { get; private set; }
        public double? Temperature { get; private set; }
        public double? HeatSetpoint { get; private set; }
        public double? CoolSetpoint { get; private set; }
        public double? Humidity { get; private set; }
        public string Mode { get; private set; }
        public string SystemStatus { get; private set; }
    }
}
