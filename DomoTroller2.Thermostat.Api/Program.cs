using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DomoTroller2.Common;
using DomoTroller2.Common.Multitenant;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.EventStore;
using DomoTroller2.Thermostat.Api.Commands;
using DomoTroller2.Thermostat.Api.Handlers;
using DomoTrollerShare2;
using DomoTrollerShare2.EventArguments;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Thermostat.Common.Command;

namespace DomoTroller2.Thermostat.Api
{
    public class Program
    {
        public static IEventStore EventStore { get; private set; }

        private static DomoShare domoShare;
        private static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            Configuration = config;
            InitConfigValues(config);
            ConnectToEventStore();
            domoShare = new DomoShare();
            domoShare.ThermostatConnected += SendEvent;
            domoShare.ThermostatHeatSetpointChanged += DomoShare_ThermostatHeatSetpointChanged;
            domoShare.ThermostatCoolSetpointChanged += DomoShare_ThermostatCoolSetpointChanged;
            domoShare.ThermostatAmbientTemperatureChanged += DomoShare_ThermostatAmbientTemperatureChanged;
            domoShare.ThermostatHumidityChanged += DomoShare_ThermostatHumidityChanged;
            domoShare.ThermostatSystemStatusChanged += DomoShare_ThermostatSystemStatusChanged;
            domoShare.ThermostatSystemModeChanged += DomoShare_ThermostatSystemModeChanged;

            CreateWebHostBuilder(args).Build().Run();
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        static void SendEvent(Object sender, ThermostatConnectedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(tenantId, e.ThermostatId, e.ThermostatGuid, 
                e.Temperature, e.HeatSetpoint, e.CoolSetpoint, e.Mode, e.SystemStatus);
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);
            //var controller = new Domain.Thermostat(eventMetadata, EventStore);
            var thermostat = Domain.Thermostat.ConnectToThermostat(eventMetadata,
                EventStore, cmd);
        }

        private static void DomoShare_ThermostatHeatSetpointChanged(object sender, ThermostatHeatSetpointChangedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            HeatSetpointChangeCommand cmd = new HeatSetpointChangeCommand(e.TenantId, e.ThermostatId, e.ThermostatGuid, e.NewHeatSetpoint);
            ChangeHeatSetpoint changeHeatSetpointCommand = new ChangeHeatSetpoint(EventStore, e.ThermostatId, e.ThermostatGuid,
                e.TenantId, (double)e.NewHeatSetpoint);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeHeatSetpointCommand);
        }

        private static void DomoShare_ThermostatCoolSetpointChanged(object sender, ThermostatCoolSetpointChangedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            CoolSetpointChangeCommand cmd = new CoolSetpointChangeCommand(e.TenantId, e.ThermostatId, e.ThermostatGuid, e.NewCoolSetpoint);
            ChangeCoolSetpoint changeCoolSetpointCommand = new ChangeCoolSetpoint(EventStore, e.ThermostatId, e.ThermostatGuid,
                e.TenantId, (double)e.NewCoolSetpoint);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeCoolSetpointCommand);
        }

        private static void DomoShare_ThermostatAmbientTemperatureChanged(object sender, ThermostatAmbientTemperatureChangedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            AmbientTemperatureChangeCommand cmd = new AmbientTemperatureChangeCommand(e.TenantId, e.ThermostatId, 
                e.ThermostatGuid, e.NewAmbientTemperature);
            ChangeAmbientTemperature changeAmbientTemperatureCommand = new ChangeAmbientTemperature(EventStore, e.ThermostatId, e.ThermostatGuid,
                e.TenantId, (double)e.NewAmbientTemperature);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeAmbientTemperatureCommand);
        }

        private static void DomoShare_ThermostatHumidityChanged(object sender, ThermostatHumidityChangedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            AmbientTemperatureChangeCommand cmd = new AmbientTemperatureChangeCommand(e.TenantId, e.ThermostatId, 
                e.ThermostatGuid, e.NewHumidity);
            ChangeHumidity changeHumidityCommand = new ChangeHumidity(EventStore, e.ThermostatId, e.ThermostatGuid,
                e.TenantId, (double)e.NewHumidity);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeHumidityCommand);
        }

        private static void DomoShare_ThermostatSystemStatusChanged(object sender, ThermostatSystemStatusChangedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            SystemStatusChangeCommand cmd = new SystemStatusChangeCommand(e.TenantId, e.ThermostatId, 
                e.ThermostatGuid, e.NewSystemStatus);
            ChangeSystemStatus changeSystemStatusCommand = new ChangeSystemStatus(EventStore, e.ThermostatId, e.ThermostatGuid,
                e.TenantId, e.NewSystemStatus);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeSystemStatusCommand);
        }

        private static void DomoShare_ThermostatSystemModeChanged(object sender, ThermostatSystemModeChangedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            SystemModeChangeCommand cmd = new SystemModeChangeCommand(e.TenantId, e.ThermostatId, 
                e.ThermostatGuid, e.NewSystemMode);
            ChangeSystemMode changeSystemModeCommand = new ChangeSystemMode(EventStore, e.ThermostatId, e.ThermostatGuid,
                e.TenantId, e.NewSystemMode);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeSystemModeCommand);
        }

        private static void ConnectToEventStore()
        {
            EventStore = EventStoreFactory.CreateEventStore();
            string connectionString = "tcp://" + ConfigManager.ServiceDiscovery.EventStoreUrl + ":" + ConfigManager.ServiceDiscovery.EventStoreTcpPort;
            EventStore.Connect(connectionString, ConfigManager.ServiceDiscovery.EventStoreUser,
                ConfigManager.ServiceDiscovery.EventStorePassword,
                ConfigManager.ServiceDiscovery.EventStoreCommonName,
                false, ConfigManager.ServiceDiscovery.EventStoreReconnectionAttempts,
                ConfigManager.EventStoreHeartbeatInterval, ConfigManager.EventStoreHeartbeatTimeout);
        }

        private static void InitConfigValues(IConfiguration config)
        {
            var eventStoreSettings = config.GetSection("EventStoreSettings");

            ConfigManager.ServiceDiscovery = new ServiceDiscoveryData()
            {
                //EventStoreCommonName = eventStoreSettings.GetSection("CertificateCommonName").Value,
                EventStoreUrl = eventStoreSettings.GetSection("EventStoreURL").Value,
                EventStoreTcpPort = int.Parse(eventStoreSettings.GetSection("TCPPort").Value),
                EventStoreUser = eventStoreSettings.GetSection("EventStoreUserName").Value,
                EventStorePassword = eventStoreSettings.GetSection("EventStorePassword").Value,
                EventStoreReconnectionAttempts = int.Parse(eventStoreSettings.GetSection("ReconnectionAttempts").Value),
            };
        }
    }
}
