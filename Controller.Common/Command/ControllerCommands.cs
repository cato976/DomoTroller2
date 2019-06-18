using System;
using DomoTroller2.Common;

namespace Controller.Common.Command
{
    public class ConnectToController : ICommand
    {
        public ConnectToController(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
