using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;
using System;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class HeatSetpointChanged : Event
    {
        public HeatSetpointChanged(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, 
            double newHeatSetpoint) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            NewHeatSetpoint = newHeatSetpoint;
        }

        [JsonConstructor]
        private HeatSetpointChanged(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, double newHeatSetpoint, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, newHeatSetpoint)
        {
            ExpectedVersion = version;
        }

        public double NewHeatSetpoint { get; private set; }
    }
}
