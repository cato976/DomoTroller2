using static DomoTrollerShare2.Enumerations.ClimateControl;

namespace DomoTrollerShare2.Item
{
    public class Thermostat
    {
        public string ID { get; set; }

        public ThermostatType ThermostatType { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public string InsideTemp { get; set; }

        public string InsideTempCel { get; set; }

        public string OutsideTemp { get; set; }

        public string OutsideTempCel { get; set; }

        public float CoolSetting { get; set; }

        public float CoolSettingCel { get; set; }

        public float HeatSetting { get; set; }

        public float HeatSettingCel { get; set; }

        public float Humidity { get; set; }

        public float Humidify { get; set; }

        public float Dehumidify { get; set; }

        public ThermostatFanMode FanMode { get; set; }

        public ThermostatHoldMode HoldMode { get; set; }

        public ThermostatMode Mode { get; set; }      
        
        public string SystemStatus { get; set; }  
        
    }
}
