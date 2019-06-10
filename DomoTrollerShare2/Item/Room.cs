using System.Collections.Generic;

namespace DomoTrollerShare2.Item
{
    public class Room : Unit
    {
        public Room()
        {
            Units = new List<Unit>();
        }
        
        public List<Unit> Units { get; set; }
    }
}
