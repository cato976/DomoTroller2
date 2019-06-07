using System;

namespace DomoTroller2.Api.Commands.Unit
{
    public class DoorOpenCommand
    {
        public DoorOpenCommand(Guid unitId)
        {
            UnitId = unitId;
        }

        public Guid UnitId { get; set; }
    }
}
