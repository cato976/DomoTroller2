using System;
using DomoTroller2.Common;

namespace Unit.Common.Command
{
    public class DoorClose : ICommand
    {
        public DoorClose(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}

