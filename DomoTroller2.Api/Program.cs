﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Controller.Common.Command;
using DomoTroller2.Api.Commands.Controller;
using DomoTroller2.Common;
using DomoTroller2.Common.Multitenant;
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
