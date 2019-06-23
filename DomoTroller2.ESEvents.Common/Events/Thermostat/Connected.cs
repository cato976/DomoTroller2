using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace DomoTroller2.ESEvents.Common.Events.Thermostat
{
    public class Connected : Event
    {
        public Connected(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, 
            double temperature, double heatSetpoint, double coolSetpoint, string mode, string systemStatus, double? humidity) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            AggregateGuid = aggregateGuid;
            Temperature = temperature;
            HeatSetpoint = heatSetpoint;
            CoolSetpoint = coolSetpoint;
            Humidity = humidity;
            Mode = mode;
            SystemStatus = systemStatus;
        }

        [JsonConstructor]
        private Connected(Guid aggregateGuid, string effectiveDateTime, string baseContentGuid,
            EventMetadata metadata, double temperature, double heatSetpoint, double coolSetpoint, string mode, string systemStatus, double? humidity, int version)
            : this(aggregateGuid, DateTimeOffset.Parse(effectiveDateTime), metadata, temperature, heatSetpoint, coolSetpoint, mode, systemStatus, humidity)
        {
            ExpectedVersion = version;
        }

        public double Temperature { get; private set; }
        public double HeatSetpoint { get; private set; }
        public double CoolSetpoint { get; private set; }
        public double? Humidity { get; private set; }
        public string Mode { get; private set; }
        public string SystemStatus { get; private set; }
    }
}
