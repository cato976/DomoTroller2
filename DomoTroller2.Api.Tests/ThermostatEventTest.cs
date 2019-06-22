using DomoTroller2.Api.Commands.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DomoTroller2.Api.Tests
{
    public class ThermostatEventTest
    {
        EventMetadata eventMetadata;
        Mock<IEventStore> moqEventStore;

        [SetUp]
        public void Setup()
        {
            eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            moqEventStore = new Mock<IEventStore>();
            moqEventStore.Setup(x => x.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()));
        }

        [Test]
        public void Connect_To_Thermostat_No_Ambient_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand("ThermostatId", Guid.NewGuid(), null, 50, 60, "Cool/Heat", "Idel");

            Assert.Throws<ArgumentNullException>(() => new Domain.Thermostat(eventMetadata, moqEventStore.Object).ConnectToThermostat(cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_Heat_Setpoint_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand("ThermostatId", Guid.NewGuid(), 70, null, 60, "Cool/Heat", "Idel");

            Assert.Throws<ArgumentNullException>(() => new Domain.Thermostat(eventMetadata, moqEventStore.Object).ConnectToThermostat(cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_Cool_Setpoint_Temperature_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand("ThermostatId", Guid.NewGuid(), 40, 50, null, "Cool/Heat", "Idel");

            Assert.Throws<ArgumentNullException>(() => new Domain.Thermostat(eventMetadata, moqEventStore.Object).ConnectToThermostat(cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_Mode_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand("ThermostatId", Guid.NewGuid(), 40, 50, 76, string.Empty, "Cooling");

            Assert.Throws<ArgumentNullException>(() => new Domain.Thermostat(eventMetadata, moqEventStore.Object).ConnectToThermostat(cmd));
        }

        [Test]
        public void Connect_To_Thermostat_No_System_Status_Should_Throw_ArgumentNullException()
        {
            ConnectToThermostatCommand cmd = new ConnectToThermostatCommand("ThermostatId", Guid.NewGuid(), 40, 50, 80, "Cool", string.Empty);

            Assert.Throws<ArgumentNullException>(() => new Domain.Thermostat(eventMetadata, moqEventStore.Object).ConnectToThermostat(cmd));
        }
    }
}
