using DomoTroller2.Common.CommandBus;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.Thermostat.Api.Commands;
using DomoTroller2.Thermostat.Api.Handlers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Thermostat.Common.Command;

namespace DomoTroller2.Thermostat.Api.Tests
{
    [TestFixture]
    public class HandlerTest
    {
        [Test]
        public void Should_Handle_Connect_To_Thermostat_Command()
        {
            PassCommandToCommandBus(new ConnectThermostat(Guid.NewGuid()));
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

        [Test]
        public void Connect_To_Thermostat_Should_Send_One_Event_To_EventStore()
        {
            var moqEventStore = new Mock<IEventStore>();
            var eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "thermostatId", Guid.NewGuid(), 56, 60, 72, "Cool", "Idel");

            var p = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);

            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        private void PassCommandToCommandBus(Common.ICommand handlerCommand)
        {
            CommandHandlerRegistration.RegisterCommandHandler();
            var commandBus = CommandBus.Instance;
            commandBus.Execute(handlerCommand);
        }
    }
}
