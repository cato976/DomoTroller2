using System;
using NUnit.Framework;
using DomoTroller2.Api.Commands.Unit;
using DomoTroller2.ESFramework.Common.Base;
using Moq;
using DomoTroller2.ESFramework.Common.Interfaces;
using System.Collections.Generic;

namespace DomoTroller2.Api.Tests
{
    [TestFixture]
    public class UnitEventsTest
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
        public void Set_Unit_Door_Open_Should_Send_One_Event_To_EventStore()
        {
            DoorOpenCommand cmd = new DoorOpenCommand(Guid.NewGuid());

            var p = new Domain.Unit(cmd.UnitId, moqEventMetadata.Object, moqEventStore.Object).DoorOpen(eventMetadata, moqEventStore.Object, cmd);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }

        [Test]
        public void Set_Unit_Door_Close_Should_Send_One_Event_To_EventStore()
        {
            DoorCloseCommand cmd = new DoorCloseCommand(Guid.NewGuid());

            var p = new Domain.Unit(cmd.UnitId, moqEventMetadata.Object, moqEventStore.Object).DoorClose(eventMetadata, moqEventStore.Object, cmd);
            moqEventStore.Verify(m => m.SaveEvents(It.IsAny<CompositeAggregateId>(), It.IsAny<IEnumerable<IEvent>>()), Times.Exactly(1));
        }
    }
}
