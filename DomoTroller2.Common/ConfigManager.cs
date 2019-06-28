using DomoTroller2.Common.Multitenant;

namespace DomoTroller2.Common
{
    public static class ConfigManager
    {
        public static ServiceDiscoveryData ServiceDiscovery;
        public static int EventStoreHeartbeatInterval = 30;
        public static int EventStoreHeartbeatTimeout = 120;
        public static int SMTPPort;
    }
}
