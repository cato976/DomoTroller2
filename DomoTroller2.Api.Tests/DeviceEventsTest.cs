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
using DomoTroller2.Api.Commands.Controller;

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
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
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
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
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
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
            {
                Level = -5
            };

            Assert.Throws<ArgumentException>(() => Domain.Device.TurnOn(moqEventMetadata.Object, moqEventStore.Object, cmd));
        }

        [Test]
        public void Should_Set_Percentage_To_100_When_Turning_On_Device_Without_Setting_Percentage()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid());

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            var events = p.GetUncommittedEvents();

            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());
            Assert.IsInstanceOf<TurnedOn>(events.First());
            TurnedOn turnedOn = events.First() as TurnedOn;
            turnedOn.Level.ShouldEqual(100);
        }

        [Test]
        public void Should_Set_Percentage_To_100_When_Turning_On_Device_Setting_Level_Over_100()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
            {
                Level = 200
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            var events = p.GetUncommittedEvents();

            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());
            Assert.IsInstanceOf<TurnedOn>(events.First());
            TurnedOn turnedOn = events.First() as TurnedOn;
            turnedOn.Level.ShouldEqual(100);
        }

        [Test]
        public void Should_Throw_Exception_When_No_Device_Id_Is_Provided()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.Empty)
            {
                Level = 70
            };

            Assert.Throws<ArgumentException>(() => Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void Should_Throw_Exception_When_Setting_Level_To_Negative_Level()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
            {
                Level = 70
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            Assert.Throws<ArgumentException>(() => p.SetLevel(eventMetadata, p.AggregateGuid, -45, p.Version));
        }

        [Test]
        public void Should_Set_Level_To_30()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
            {
                Level = 70
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            p.SetLevel(eventMetadata, p.AggregateGuid, 30, p.Version);
            var events = p.GetUncommittedEvents();
            events.Count().ShouldEqual(2);
            Assert.IsInstanceOf<SetLevel>(events.Last());
            SetLevel setLevel = events.Last() as SetLevel;
            setLevel.Level.ShouldEqual(30);
        }

        [Test]
        public void Turn_On_Device_Should_Send_One_Event_To_EventStore()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
            {
                Level = 9
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }

        [Test]
        public void Set_Device_Level_Should_Send_Two_Event_To_EventStore()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid())
            {
                Level = 70
            };

            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            p.SetLevel(eventMetadata, p.AggregateGuid, 45, p.Version);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(2));
        }

        [Test]
        public void Should_Throw_Exception_From_Event_Store_When_Turning_On_Device()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid());

            moqEventStore.Setup(x => x.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>())).Throws(new Exception("Problem in Event Store"));
            Assert.Throws<Exception>(() => Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd));
        }

        [Test]
        public void Set_Level_Version_Mismatch_Should_Throw_ArgumentOutOfReangeException()
        {
            TurnOnCommand cmd = new TurnOnCommand(Guid.NewGuid());
            var p = Domain.Device.TurnOn(eventMetadata, moqEventStore.Object, cmd);
            Assert.Throws<ArgumentOutOfRangeException>(() => p.SetLevel(eventMetadata, p.AggregateGuid, 80, 100));
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Once);
        }
    }
}
