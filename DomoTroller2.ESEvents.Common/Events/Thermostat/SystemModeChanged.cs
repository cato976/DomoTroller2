using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class SystemModeChanged : Event
    {
        public SystemModeChanged(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata,
            string newMode) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            NewSystemMode = newMode;
        }

        [JsonConstructor]
        private SystemModeChanged(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, string newSystemMode, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, newSystemMode)
        {
            ExpectedVersion = version;
        }

        public string NewSystemMode { get; private set; }
    }
}
