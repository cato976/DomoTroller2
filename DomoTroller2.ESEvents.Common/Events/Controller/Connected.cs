using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Controller
{
    public class Connected : Event
    {
        public Connected(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
        }

        [JsonConstructor]
        private Connected(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid, string description, EventMetadata metadata, int version) : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata)
        {
            Version = version;
        }
    }
}
