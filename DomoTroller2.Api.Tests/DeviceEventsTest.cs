using System;
using System.Collections.Generic;
using DomoTroller2.Api.Commands.Device;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Moq;
using NUnit.Framework;
using Should;
using System.Linq;

namespace DomoTroller2.Api.Tests
{
    [TestFixture]
    public class DeviceEventsTest
    {
        Mock<IEventStore> moqEventStore;
        Mock<IEventMetadata> moqEventMetadata;
        EventMetadata eventMetadata;

        [SetUp]
        public void Setup()
        {
            moqEventMetadata = new Mock<IEventMetadata>();
            moqEventStore = new Mock<IEventStore>();
            moqEventStore.Setup(x => x.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()));
            eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        }

        [Test]
        public void Should_Generate_TurnedOn_Event_When_Turning_A_Device_On()
        {
            TurnOnCommand cmd = new TurnOnCommand()
            {
                Level = 45
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);

            var events = p.GetUncommittedEvents();

            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());
            Assert.IsInstanceOf<TurnedOn>(events.First());
        }

        [Test]
        public void Should_Generate_TurnedOn_Event_When_Turn_On_Device_Command_Is_Sent()
        {
            TurnOnCommand cmd = new TurnOnCommand()
            {
                Level = 90
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            var events = p.GetUncommittedEvents();

            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());
            Assert.IsInstanceOf<TurnedOn>(events.First());
        }

        [Test]
        public void Should_Throw_Exception_When_Turning_On_Device_With_Negative_Percentage()
        {
            TurnOnCommand cmd = new TurnOnCommand()
            {
                Level = -5
            };

            Assert.Throws<ArgumentException>(() => Domain.Device.TurnOn(moqEventMetadata.Object, moqEventStore.Object, cmd));
        }

        [Test]
        public void Should_Set_Percentage_To_100_When_Turning_On_Device_Without_Setting_Percentage()
        {
            TurnOnCommand cmd = new TurnOnCommand();

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            var events = p.GetUncommittedEvents();

            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());
            Assert.IsInstanceOf<TurnedOn>(events.First());
            TurnedOn turnedOn = events.First() as TurnedOn;
            turnedOn.Percentage.ShouldEqual(100);
        }

        [Test]
        public void Should_Throw_Exception_When_Setting_Level_To_Negative_Level()
        {
            TurnOnCommand cmd = new TurnOnCommand()
            {
                Level = 70
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            Assert.Throws<ArgumentException>(() => p.SetLevel(eventMetadata, -45, p.Version));
        }

        [Test]
        public void Turn_On_Device_Should_Send_One_Event_To_EventStore()
        {
            TurnOnCommand cmd = new TurnOnCommand()
            {
                Level = 9
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [Test]
        public void Change_First_Name_Should_Send_Two_Event_To_EventStore()
        {
            TurnOnCommand cmd = new TurnOnCommand()
            {
                Level = 70
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            p.SetLevel(eventMetadata, 45, p.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
        }

        [Test]
        public void Should_Throw_Exception_From_Event_Store_When_Turning_On_Device()
        {
            TurnOnCommand cmd = new TurnOnCommand();

            moqEventStore.Setup(x => x.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>())).Throws(new Exception("Problem in Event Store"));
            Assert.Throws<Exception>(() => Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd));
        }
    }
}
