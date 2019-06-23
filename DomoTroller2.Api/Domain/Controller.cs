using System;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.ESEvents.Common.Events.Controller;
using Controller.Common.Command;
using DomoTroller2.EventStore.Exceptions;
using System.Diagnostics;
using DomoTroller2.Api.Commands.Controller;

namespace DomoTroller2.Api.Domain
{
    public class Controller : Aggregate
    {
        private Controller()
        {
        }

        public Controller(IEventMetadata eventMetadata, IEventStore eventStore)
        {
            EventMetadata = eventMetadata;
            EventStore = eventStore;
        }

        private Controller(Guid id, IEventMetadata eventMetadata, IEventStore eventStore)
        {
            AggregateGuid = id; 
            ApplyEvent(new Connected(id, DateTimeOffset.UtcNow, eventMetadata));

            // Send Event to Event Store
            var events = this.GetUncommittedEvents();
            try
            {
                EventSender.SendEvent(eventStore, new CompositeAggregateId(eventMetadata.TenantId, AggregateGuid, eventMetadata.Category), events);
            }
            catch (ConnectionFailure conn)
            {
                Trace.TraceError($"There was a connection error: {conn}");
            }
        }

        private new readonly IEventMetadata EventMetadata;
        private readonly IEventStore EventStore;

        public Controller ConnectToController(ConnectToControllerCommand cmd)
        {
            var controllerId = cmd.ControllerId;
            var connectToControllerCommand = new ConnectToController(controllerId);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(connectToControllerCommand);
            var controller = new Controller(controllerId, EventMetadata, EventStore);
            return controller;
        }
    }
}
