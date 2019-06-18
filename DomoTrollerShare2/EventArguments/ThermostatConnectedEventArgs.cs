using System;

namespace DomoTrollerShare2.EventArguments
{
    public class ThermostatConnectedEventArgs : EventArgs
    {
        public ThermostatConnectedEventArgs(string thermostatId)
        {
            ThermostatId = thermostatId;
        }
        public string ThermostatId { get; private set; }
    }
}
