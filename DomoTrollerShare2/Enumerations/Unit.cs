using System.ComponentModel.DataAnnotations;

namespace DomoTrollerShare2.Enumerations
{
    public class Unit
    {
        public enum Command
        {
            Off,
            On,
            Toggle,
            Dim,
            Brighten,
            [Display(Name = "Light Level")]
            LightLevel,
            Ramp,
            Scene
        }
        
        public enum Time
        {
            Indefinitely,
            Seconds,
            Minutes,
            Hours
        }

        public enum UnitType
        {
            Unit,
            Room
        }
    }
}
