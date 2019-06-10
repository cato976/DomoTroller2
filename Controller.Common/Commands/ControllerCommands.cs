using System;
using DomoTroller2.Common;

namespace Controller.Common.Commands
{
    public class ConnectToControllerCommand : ICommand
    {
        public ConnectToControllerCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
