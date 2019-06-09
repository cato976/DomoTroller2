using System.Collections.Generic;
using DomoTrollerShare2.Item;

namespace DomoTrollerShare2
{
    public class Status
    {
        public Status()
        {
            Areas = new List<Area>();
            Thermostats = new List<Thermostat>();
            Zones = new List<Zone>();
            Rooms = new List<Room>();
            Units = new List<Unit>();
        }

        public List<Area> Areas { get; set; }

        public List<Thermostat> Thermostats { get; set; }

        public List<Zone> Zones { get; set; }

        public List<Room> Rooms { get; set; }

        public List<Unit> Units { get; set; }
        
    }
}
