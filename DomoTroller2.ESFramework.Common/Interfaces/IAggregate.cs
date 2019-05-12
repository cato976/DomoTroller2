using System.Collections.Generic;
using DomoTroller2.ESFramework.Common.Base;

namespace DomoTroller2.ESFramework.Common.Interfaces
{
    public interface IAggregate
    {
        IEnumerable<Event> GetUncommittedEvents();
    }
}
