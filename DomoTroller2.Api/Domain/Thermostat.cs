using DomoTroller2.Api.Commands.Thermostat;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.ESEvents.Common.Events.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.EventStore.Exceptions;
using System;
using System.Diagnostics;
using Thermostat.Common.Command;

namespace DomoTroller2.Api.Domain
{
    public class Thermostat : Aggregate
    {
        public Thermostat(IEventMetadata eventMetadata, IEventStore eventStore)
        {
            EventMetadata = eventMetadata;
            EventStore = eventStore;
        }

        private Thermostat(Guid id, IEventMetadata eventMetadata, IEventStore eventStore)
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

        private readonly IEventMetadata EventMetadata;
        private readonly IEventStore EventStore;

        public Thermostat ConnectToThermostat(ConnectToThermostatCommand cmd)
        {
            var thermostatId = cmd.ThermostatId;
            var connectToControllerCommand = new ConnectThermostat(thermostatId);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(connectToControllerCommand);
            var thermostat = new Thermostat(thermostatId, EventMetadata, EventStore);
            return thermostat;
        }
    }
}
