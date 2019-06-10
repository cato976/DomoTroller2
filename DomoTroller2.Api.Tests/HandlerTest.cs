using System;
using System.Linq;
using Controller.Common.Commands;
using DomoTroller2.Api.Handlers;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.Common.EventBus;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESEvents.Common.Events.Unit;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Moq;
using NUnit.Framework;
using Should;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace DomoTroller2.Api.Tests
{
    [TestFixture]
    public class HandlerTest
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
        public void Should_Handle_Device_Turned_On_Event()
        {
            PassEventToEventBus(new TurnedOn(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 90));
        }

        [Test]
        public void Should_Handle_Device_SetLevel_Event()
        {
            PassEventToEventBus(new SetLevel(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 90));
        }

        [Test]
        public void Should_Handle_Turn_On_Device_Command()
        {
            PassCommandToCommandBus(new Device.Common.Command.TurnOn(Guid.NewGuid(), 10));
        }

        [Test]
        public void Should_Handle_SetLevel_Device_Command()
        {
            PassCommandToCommandBus(new Device.Common.Command.SetLevel(Guid.NewGuid(), 10));
        }

        [Test]
        public void Should_Handle_Unit_Door_Opened_Event()
        {
            PassEventToEventBus(new DoorOpened(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata));
        }

        [Test]
        public void Should_Handle_Door_OpenTurn_Unit_Command()
        {
            PassCommandToCommandBus(new Unit.Common.Command.DoorOpen(Guid.NewGuid()));
        }

        [Test]
        public void Should_Connect_To_Controller()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToControllerCommand cmd = new ConnectToControllerCommand(Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value));

            var p = new Controller(eventMetadata, moqEventStore.Object).ConnectToController(cmd);

            var events = p.GetUncommittedEvents();

            events.ShouldNotBeEmpty();
            events.Count().ShouldBeGreaterThan(0);
            events.Count().ShouldBeLessThan(2);
            Assert.IsNotNull(p.AggregateGuid);
            Assert.AreNotEqual(Guid.Empty, p.AggregateGuid);
        }

        [Test]
        public void Connect_To_Controller_Should_Send_One_Event_To_EventStore()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToControllerCommand cmd = new ConnectToControllerCommand(Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value));

            var p = new Controller(eventMetadata, moqEventStore.Object).ConnectToController(cmd);

            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        private void PassEventToEventBus(IEvent handledEvent)
        {
            EventStoreHandlerRegistration.RegisterEventHandler(moqEventStore.Object);
            var eventBus = EventBus.Instance;
            eventBus.Execute(handledEvent);
        }

        private void PassCommandToCommandBus(DomoTroller2.Common.ICommand handlerCommand)
        {
            CommandHandlerRegistration.RegisterCommandHandler();
            var commandBus = CommandBus.Instance;
            commandBus.Execute(handlerCommand);
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
