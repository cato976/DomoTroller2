using System;

namespace DomoTroller2.Api.Commands.Controller
{
    public class ConnectToControllerCommand
    {
        public ConnectToControllerCommand(Guid controllerId)
        {
            ControllerId = controllerId;
        }

        public Guid ControllerId { get; set; }
    }
}
