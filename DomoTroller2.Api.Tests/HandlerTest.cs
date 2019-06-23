using System;
using System.Linq;
using Controller.Common.Command;
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
using DomoTroller2.Api.Commands.Controller;
using DomoTroller2.ESEvents.Common.Events.Controller;
using DomoTroller2.Api.Commands.Thermostat;
using Thermostat.Common.Command;
using DomoTroller2.Api.Handlers.Thermostat;

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
        public void Should_Handle_Controller_Connect_Event()
        {
            PassEventToEventBus(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata));
        }

        [Test]
        public void Should_Handle_Door_OpenTurn_Unit_Command()
        {
            PassCommandToCommandBus(new Unit.Common.Command.DoorOpen(Guid.NewGuid()));
        }

        [Test]
        public void Should_Handle_Connect_To_Controller_Command()
        {
            PassCommandToCommandBus(new ConnectToController(Guid.NewGuid()));
        }

        [Test]
        public void Should_Handle_Thermostat_Connect_Event()
        {
            PassEventToEventBus(new ESEvents.Common.Events.Thermostat.Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 
                76, 60, 76, "Cool", "Cooling", 60));
        }

        [Test]
        public void Should_Connect_To_Controller()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToControllerCommand cmd = new ConnectToControllerCommand(Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value));

            var p = new Domain.Controller(eventMetadata, moqEventStore.Object).ConnectToController(cmd);

            var events = p.GetUncommittedEvents();

            events.ShouldNotBeEmpty();
            events.Count().ShouldBeGreaterThan(0);
            events.Count().ShouldBeLessThan(2);
            Assert.IsNotNull(p.AggregateGuid);
            Assert.AreNotEqual(Guid.Empty, p.AggregateGuid);
        }

        [Test]
        public void Should_Handle_Connect_To_Thermostat_Command()
        {
            PassCommandToCommandBus(new ConnectThermostat(Guid.NewGuid()));
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

        [Test]
        public void Should_Connect_To_Thermostat()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "theremostatId", Guid.NewGuid(), 76, 60.9, 77.2, "Heat", "Heating", 77.4);
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new ESEvents.Common.Events.Thermostat.Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var p = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);

            var events = p.GetUncommittedEvents();

            Assert.IsNotNull(p.AggregateGuid);
            Assert.AreNotEqual(Guid.Empty, p.AggregateGuid);
        }

        [Test]
        public void Should_Change_Heat_Setpoint_To_Thermostat()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var thermostatId = "termostatId";
            var thermostatGuid = Guid.NewGuid();
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), thermostatId, thermostatGuid, 76, 60.9, 77.2, "Heat", "Heating", 77.4);
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new ESEvents.Common.Events.Thermostat.Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var p = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            HeatSetpointChangeCommand heatSetpointChangeCommand = new HeatSetpointChangeCommand(Guid.NewGuid(), thermostatId, thermostatGuid, 61);
            ChangeHeatSetpoint changeHeatSetpointCommand = new ChangeHeatSetpoint(moqEventStore.Object, thermostatId, thermostatGuid, Guid.NewGuid(), (double)cmd.HeatSetpoint);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeHeatSetpointCommand);

            Assert.IsNotNull(p.AggregateGuid);
            Assert.AreNotEqual(Guid.Empty, p.AggregateGuid);
        }

        [Test]
        public void Should_Change_Cool_Setpoint_To_Thermostat()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var thermostatId = "termostatId";
            var thermostatGuid = Guid.NewGuid();
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), thermostatId, thermostatGuid, 76, 60.9, 77.2, "Heat", "Heating", 77.4);
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new ESEvents.Common.Events.Thermostat.Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var p = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            CoolSetpointChangeCommand coolSetpointChangeCommand = new CoolSetpointChangeCommand(Guid.NewGuid(), thermostatId, thermostatGuid, 61);
            ChangeCoolSetpoint changeHeatSetpointCommand = new ChangeCoolSetpoint(moqEventStore.Object, thermostatId, thermostatGuid, Guid.NewGuid(), (double)cmd.HeatSetpoint);
            var handler = new ThermostatCommandHandlers();
            handler.Handle(changeHeatSetpointCommand);

            Assert.IsNotNull(p.AggregateGuid);
            Assert.AreNotEqual(Guid.Empty, p.AggregateGuid);
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

        [Test]
        public void Connect_To_Thermostat_Should_Send_One_Event_To_EventStore()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "thermostatId", Guid.NewGuid(), 56, 60, 72, "Cool", "Idel");

            var p = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);

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
