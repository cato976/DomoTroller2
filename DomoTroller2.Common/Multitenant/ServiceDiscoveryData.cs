using Newtonsoft.Json;

namespace DomoTroller2.Common.Multitenant
{
    public class ServiceDiscoveryData
    {
        public ServiceDiscoveryData()
        {
            EventStoreCommonName = string.Empty;
        }

        [JsonProperty("eventStoreUrl")]
        public string EventStoreUrl { get; set; }

        [JsonProperty("eventStoreTcpPort")]
        public int EventStoreTcpPort { get; set; }

        [JsonProperty("eventStoreUser")]
        public string EventStoreUser { get; set; }

        [JsonProperty("eventStorePassword")]
        public string EventStorePassword { get; set; }

        [JsonProperty("eventStoreCommonName")]
        public string EventStoreCommonName { get; set; }
        
        [JsonProperty("eventStoreReconnectionAttempts")]
        public int EventStoreReconnectionAttempts { get; set; }
    }
}
