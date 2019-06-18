using System;
using System.Runtime.Serialization;

namespace NestSharp
{
    public enum ColorState
    {
        Gray,
        Green,
        Yellow,
        Red
    }

    public enum CoAlarmState
    {
        Ok,
        Warning,
        Emergency
    }

    public enum BatteryHealth
    {
        Ok,
        Replace
    }

    public enum TemperatureScale
    {
        C,
        F
    }

    public enum HvacMode
    {
        Heat,
        Cool,
        [EnumMember(Value = "heat-cool")]
        HeatCool,
        Eco,
        Off
    }

    public enum HvacState
    {
        Off,
        Heating,
        Cooling
    }

    public enum TemperatureSettingType
    {
        None,
        High,
        Low
    }

    public enum Away 
    {
        Home,
        Away,
        AutoAway,
        Unknown
    }
}

