using System;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.ESFramework.Common.Base
{
    public class Event : IEvent
    {
        public Event(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata)
        {
            if (effectiveDateTime == DateTime.MinValue)
            {
                throw new ArgumentException(nameof(effectiveDateTime));
            }

            AggregateGuid = aggregateGuid;
            EffectiveDateTime = effectiveDateTime;
            Metadata = metadata as EventMetadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public Guid AggregateGuid { get; set; }
        public EventMetadata Metadata { get; }
        public DateTimeOffset EffectiveDateTime { get; }
        public long ExpectedVersion { get; set; }
        //public long CurrentEventNumber { get; set; }
    }
}
