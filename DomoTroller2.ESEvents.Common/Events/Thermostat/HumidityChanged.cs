using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class HumidityChanged : Event
    {
        public HumidityChanged(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata,
            double newHumidity) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            NewHumidity = newHumidity;
        }

        [JsonConstructor]
        private HumidityChanged(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, double newHumidity, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, newHumidity)
        {
            ExpectedVersion = version;
        }

        public double NewHumidity { get; private set; }
    }
}
