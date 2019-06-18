using DomoTroller2.Api.Commands.Controller;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DomoTroller2.Api.Tests
{
    [TestFixture]
    public class ControllerEventsTest
    {
        Mock<IEventStore> moqEventStore;
        IEventMetadata eventMetadata;
        private static IConfigurationRoot Configuration;

        [SetUp]
        public void Setup()
        {
            moqEventStore = new Mock<IEventStore>();
            eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

            string path = GetPath();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(path))
                .AddJsonFile("appsettings.json", false, true);
            Configuration = builder.Build();
        }

        [Test]
        public void Connect_To_Controller_Should_Send_One_Event_To_EventStore()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToControllerCommand cmd = new ConnectToControllerCommand(Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value));

            var p = new Domain.Controller(eventMetadata, moqEventStore.Object).ConnectToController(cmd);

            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        public static string GetPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return path;
        }
    }
}
