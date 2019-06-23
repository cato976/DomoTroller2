using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Controller.Common.Command;
using DomoTroller2.Api.Commands.Controller;
using DomoTroller2.Api.Commands.Thermostat;
using DomoTroller2.Api.Handlers.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.EventStore;
using DomoTrollerShare2;
using DomoTrollerShare2.EventArguments;
using DomoTrollerShare2.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Thermostat.Common.Command;

namespace DomoTroller2.Api
{
    public class Program
    {
        public static IEventStore EventStore { get; private set; }
        private static DomoShare domoShare;
        private static IConfigurationRoot Configuration { get; set; }
        private static IHAC HAC { get; }

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
            domoShare.ControllerConnected += SendEvent;
            domoShare.ThermostatConnected += SendEvent;
            domoShare.ThermostatHeatSetpointChanged += DomoShare_ThermostatHeatSetpointChanged;
            domoShare.ThermostatCoolSetpointChanged += DomoShare_ThermostatCoolSetpointChanged;

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();


        static void SendEvent(Object sender, ControllerConnectedEventArgs e)
        {
            var tenantId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
            ConnectToControllerCommand cmd = new ConnectToControllerCommand(tenantId);
            var eventMetadata = new EventMetadata(cmd.ControllerId, "Controller", cmd.ControllerId.ToString(), cmd.ControllerId, cmd.ControllerId, DateTimeOffset.UtcNow);
            var controller = new Domain.Controller(eventMetadata, EventStore).ConnectToController(cmd);
        }

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

            ConfigManager.ServiceDiscovery = new Multitenant.ServiceDiscoveryData()
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
