using System;

namespace DomoTroller2.Api.Commands.Unit
{
    public class DoorCloseCommand
    {
        public DoorCloseCommand(Guid unitId)
        {
            UnitId = unitId;
        }

        public Guid UnitId { get; set; }
    }
}
