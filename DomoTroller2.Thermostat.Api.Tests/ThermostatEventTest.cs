﻿using DomoTroller2.ESEvents.Common.Events.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.Thermostat.Api.Commands;
using DomoTroller2.Thermostat.Api.Handlers;
using Moq;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using Thermostat.Common.Command;

namespace DomoTroller2.Thermostat.Api.Tests
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
            connected.Add(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var thermo = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            thermo.ChangeHeatSetpoint(eventMetadata, moqEventStore.Object, heatSetpointCmd, thermo.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
            var events = thermo.GetUncommittedEvents();
            events.Count().ShouldEqual(1);
        }

        [Test]
        public void Ambient_Temperature_Changed_Should_Update_Ambient_Tempurature()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            AmbientTemperatureChangeCommand ambientTemperatureChangeCmd = new AmbientTemperatureChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 67);
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var thermo = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            thermo.ChangeAmbientTemperature(eventMetadata, moqEventStore.Object, ambientTemperatureChangeCmd, thermo.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
            var events = thermo.GetUncommittedEvents();
            events.Count().ShouldEqual(1);
        }

        [Test]
        public void Humidity_Changed_Should_Update_Humidity()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            HumidityChangeCommand humidityChangeCmd = new HumidityChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 67);
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var thermo = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            thermo.ChangeHumidity(eventMetadata, moqEventStore.Object, humidityChangeCmd, thermo.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
            var events = thermo.GetUncommittedEvents();
            events.Count().ShouldEqual(1);
        }

        [Test]
        public void HVAC_State_Changed_Should_Update_HVAC_State()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            SystemStatusChangeCommand stateChangeCmd = new SystemStatusChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), "Running");
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var thermo = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            thermo.ChangeSystemState(eventMetadata, moqEventStore.Object, stateChangeCmd, thermo.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
            var events = thermo.GetUncommittedEvents();
            events.Count().ShouldEqual(1);
        }

        [Test]
        public void HVAC_Mode_Changed_Should_Update_HVAC_Mode()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", "Idel");
            SystemModeChangeCommand stateChangeCmd = new SystemModeChangeCommand(Guid.NewGuid(), "ThermostatId", Guid.NewGuid(), "Off");
            List<IEvent> connected = new List<IEvent>();
            connected.Add(new Connected(Guid.NewGuid(), DateTimeOffset.UtcNow, eventMetadata, 78, 56, 76, "Off", "Idel", 80));
            moqEventStore.Setup(storage => storage.GetAllEvents(It.IsAny<CompositeAggregateId>())).Returns(connected);

            var thermo = Domain.Thermostat.ConnectToThermostat(eventMetadata, moqEventStore.Object, cmd);
            thermo.ChangeSystemMode(eventMetadata, moqEventStore.Object, stateChangeCmd, thermo.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
            var events = thermo.GetUncommittedEvents();
            events.Count().ShouldEqual(1);
        }
    }
}
