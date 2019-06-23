using System;
using DomoTroller2.ESFramework.Common.Base;

namespace DomoTroller2.ESFramework.Common.Interfaces
{
    public interface IEvent
    {
        Guid AggregateGuid { get;  }
        EventMetadata Metadata { get; }
        DateTimeOffset EffectiveDateTime { get; }
        long ExpectedVersion { get; }
        //long EventNumber { get; }
    }
}
