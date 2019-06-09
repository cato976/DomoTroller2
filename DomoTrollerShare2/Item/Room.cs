using System.Collections.Generic;

namespace DomoTrollerShare2.Item
{
    public class Room
    {
        public Room()
        {
            Units = new List<Unit>();
        }
        
        public List<Unit> Units { get; set; }
    }
}
