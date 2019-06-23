using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;
using System;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class CoolSetpointChanged : Event
    {
        public CoolSetpointChanged(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, 
            double newCoolSetpoint) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            NewCoolSetpoint = newCoolSetpoint;
        }

        [JsonConstructor]
        private CoolSetpointChanged(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, double newHeatSetpoint, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, newHeatSetpoint)
        {
            ExpectedVersion = version;
        }

        public double NewCoolSetpoint { get; private set; }
    }
}
