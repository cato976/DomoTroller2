using DomoTroller2.Api.Commands.Thermostat;
using DomoTroller2.Api.Handlers.Thermostat;
using DomoTroller2.ESEvents.Common.Events.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Thermostat.Common.Command;

namespace DomoTroller2.Api.Tests
{
    public class ThermostatEventTest
    {
        EventMetadata eventMetadata;
        Mock<IEventStore> moqEventStore;

        [SetUp]
        public void Setup()
        {
            eventMetadata = new EventMetadata(Guid.NewGuid(), "Thermostat", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            moqEventStore = new Mock<IEventStore>();
            moqEventStore.Setup(x => x.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()));
        }

        [Test]
        public void Connect_To_Thermostat_No_Ambient_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), null, 50, 60, "Cool/Heat", "Idel");

            Assert.Throws<ArgumentNullException>(() => Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_Heat_Setpoint_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 70, null, 60, "Cool/Heat", "Idel");

            Assert.Throws<ArgumentNullException>(() => Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_Cool_Setpoint_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, null, "Cool/Heat", "Idel");

            Assert.Throws<ArgumentNullException>(() => Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_Mode_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 76, string.Empty, "Cooling");

            Assert.Throws<ArgumentNullException>(() => Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_System_Status_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", string.Empty);

            Assert.Throws<ArgumentNullException>(() => Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void HeatSetpoint_Changed_No_Heat_Setpoint_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            HeatSetpointChangeCommand heatSetpointCmd = new HeatSetpointChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), null);

            ChangeHeatSetpoint changeHeatSetpointCommand = new ChangeHeatSetpoint(moqEventStore.Object, cmd.ThermostatId, 
                cmd.ThermostatAggregateId, Guid.NewGuid(), heatSetpointCmd.NewHeatSetpoint);
            var handler = new ThermostatCommandHandlers();
            Assert.Throws<ArgumentNullException>(() => handler.Handle(changeHeatSetpointCommand));
        }

        [Test]
        public void CoolSetpoint_Changed_No_Cool_Setpoint_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            CoolSetpointChangeCommand coolSetpointCmd = new CoolSetpointChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), null);

            ChangeCoolSetpoint changeCoolSetpointCommand = new ChangeCoolSetpoint(moqEventStore.Object, cmd.ThermostatId, 
                cmd.ThermostatAggregateId, Guid.NewGuid(), coolSetpointCmd.NewCoolSetpoint);
            var handler = new ThermostatCommandHandlers();
            Assert.Throws<ArgumentNullException>(() => handler.Handle(changeCoolSetpointCommand));
        }

        [Test]
        public void Change_Heat_Setpoint_EventNumber_Mismatch_Should_Throw_ArgumentOutOfRangeException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            HeatSetpointChangeCommand heatSetpointCmd = new HeatSetpointChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 67);

            List<IEvent> connected = new List<IEvent>();
            connected.Add(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);
            var p = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            Assert.Throws<ArgumentOutOfRangeException>(() => p.ChangeHeatSetpoint(eventMetadata, moqEventStore.Object, heatSetpointCmd, 200));
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [Test]
        public void Change_Heat_Setpoint_TenanetId_Is_Empty__Should_Throw_ArgumentOutOfRangeException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.Empty, "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            HeatSetpointChangeCommand heatSetpointCmd = new HeatSetpointChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 67);

            Assert.Throws<ArgumentOutOfRangeException> (() => Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void HeatSetpoint_Changed_Should_Change_Heat_Setpoint()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            HeatSetpointChangeCommand heatSetpointCmd = new HeatSetpointChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 67);
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new ESEvents.Common.Events.Thermostat.Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var thermo = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            thermo.ChangeHeatSetpoint(eventMetadata, moqEventStore.Object, heatSetpointCmd, thermo.Version);
            ChangeHeatSetpoint changeHeatSetpointCommand = new ChangeHeatSetpoint(moqEventStore.Object, cmd.ThermostatId, 
                cmd.ThermostatAggregateId, Guid.NewGuid(), heatSetpointCmd.NewHeatSetpoint);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
        }
    }
}
