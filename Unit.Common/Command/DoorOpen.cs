using System;
using DomoTroller2.Common;

namespace Unit.Common.Command
{
    public class DoorOpen : ICommand
    {
        public DoorOpen(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
