using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Device
{
    public class SetLevel : Event
    {
        public SetLevel(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, int level) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            Level = level;
        }

        [JsonConstructor]
        private SetLevel(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid, string description, EventMetadata metadata, int level, int version) : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, level)
        {
            Version = version;
        }

        public int Level { get; private set; }
    }
}
