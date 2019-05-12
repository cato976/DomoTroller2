using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Device
{
    public class TurnedOn : Event
    {
        public TurnedOn(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, int percentage) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            Percentage = percentage;
        }

        [JsonConstructor]
        private TurnedOn(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid, string description, EventMetadata metadata, int percentage, int version) : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, percentage)
        {
            Version = version;
        }

        public int Percentage { get; private set; }
    }
}
