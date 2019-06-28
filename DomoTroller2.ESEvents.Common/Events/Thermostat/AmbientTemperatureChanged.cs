using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class AmbientTemperatureChanged : Event
    {
        public AmbientTemperatureChanged(Guid aggregateGuid, DateTimeOffset effectiveDateTime,
            IEventMetadata metadata, double newAmbientTemperature) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            NewAmbientTemperature = newAmbientTemperature;
        }

        [JsonConstructor]
        private AmbientTemperatureChanged(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, double newAmbientTemperature, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, newAmbientTemperature)
        {
            ExpectedVersion = version;
        }

        public double NewAmbientTemperature { get; private set; }
    }
}
