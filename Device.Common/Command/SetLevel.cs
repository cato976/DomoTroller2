using System;
using DomoTroller2.Common;

namespace Device.Common.Command
{
    public class SetLevel : ICommand
    {

        public SetLevel(Guid id, int level)
        {
            Id = id;
            Level = level;
        }

        public Guid Id { get; private set; }
        public int Level { get; private set; }
    }
}
