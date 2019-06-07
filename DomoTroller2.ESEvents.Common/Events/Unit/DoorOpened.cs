using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Unit
{
    public class DoorOpened : Event
    {
        public DoorOpened(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
        }

        [JsonConstructor]
        private DoorOpened(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid, string description, EventMetadata metadata, int version) : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata)
        {
            Version = version;
        }
    }
}
