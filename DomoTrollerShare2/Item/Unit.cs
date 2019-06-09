using static DomoTrollerShare2.Enumerations.Unit;

namespace DomoTrollerShare2.Item
{
    public class Unit
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Level { get; set; }
        public int Time { get; set; }

        public Command Command { get; set; }

        public Time Duration { get; set; }

        public UnitType UnitType { get; set; }
    }
}
