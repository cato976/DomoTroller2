using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DomoTrollerShare2.Enumerations
{
    public class ClimateControl
    {
        public enum ThermostatType { Unknown, HAI, Nest }

        public enum ThermostatFanMode
        {
            Off,
            Cycle,
            Auto,
            On
        }

        public enum ThermostatMode
        {
            Off,
            Heat,
            Cool,
            [EnumMember(Value = "heat-cool"), Display(Name = "Heat-Cool")]
            HeatCool,
            [EnumMember(Value = "Auto")]
            Auto,
            [Display(Name = "Emergency Heat")]
            EmergencyHeat,
            Eco
        }

        public enum ThermostatHoldMode
        {
            Off,
            On,
            Vacation
        }
    }
}
