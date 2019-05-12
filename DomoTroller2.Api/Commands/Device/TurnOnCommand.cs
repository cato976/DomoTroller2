using System;

namespace DomoTroller2.Api.Commands.Device
{
    public class TurnOnCommand
    {
        public TurnOnCommand(Guid deviceId)
        {
            DeviceId = deviceId;
            Level = 100;
        }

        public Guid DeviceId { get; set; }
        public int Level { get; set; }
    }
}
