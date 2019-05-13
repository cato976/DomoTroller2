using System;
using DomoTroller2.Api.Handlers;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.Common.EventBus;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using Moq;
using NUnit.Framework;

namespace DomoTroller2.Api.Tests
{
    [TestFixture]
    public class HandlerTest
    {
        Mock<IEventStore> moqEventStore;
        IEventMetadata eventMetadata;

        [SetUp]
        public void Setup()
        {
            moqEventStore = new Mock<IEventStore>();
            eventMetadata = new EventMetadata(Guid.NewGuid(), "TestCategory", "TestCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
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
    }
}
