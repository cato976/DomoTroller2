using DomoTroller2.Api.Multitenant;

namespace DomoTroller2.Api
{
    public static class ConfigManager
    {
        internal static ServiceDiscoveryData ServiceDiscovery;
        internal static int EventStoreHeartbeatInterval = 30;
        internal static int EventStoreHeartbeatTimeout = 120;
    }
}
