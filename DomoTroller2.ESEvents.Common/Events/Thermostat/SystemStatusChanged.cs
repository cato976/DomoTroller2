using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class SystemStatusChanged : Event
    {
        public SystemStatusChanged(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, 
            string newSystemStatus) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            NewSystemStatus = newSystemStatus;
        }

        [JsonConstructor]
        private SystemStatusChanged(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, string newSystemStatus, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, newSystemStatus)
        {
            ExpectedVersion = version;
        }

        public string NewSystemStatus { get; private set; }
    }
}
