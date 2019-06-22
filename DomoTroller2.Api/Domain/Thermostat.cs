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

        private Thermostat(Guid id, IEventMetadata eventMetadata, IEventStore eventStore,
            double temperature, double heatSetpoint, double coolSetpoint, string mode, string systemStatus, double? humidity = null)
        {
            AggregateGuid = id;
            ApplyEvent(new Connected(id, DateTimeOffset.UtcNow, eventMetadata, temperature, 
                heatSetpoint, coolSetpoint, mode, systemStatus, humidity));

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
            ValidateTemputure(cmd.Temperature);
            ValidateHeatSetpoint(cmd.HeatSetpoint);
            ValidateCoolSetpoint(cmd.CoolSetpoint);
            ValidateMode(cmd.Mode);
            ValidateSystemStatus(cmd.SystemStatus);

            var thermostatId = cmd.ThermostatId;
            var connectToControllerCommand = new ConnectThermostat(cmd.ThermostatAggregateId);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(connectToControllerCommand);
            var thermostat = new Thermostat(cmd.ThermostatAggregateId, EventMetadata, EventStore, 
                (double)cmd.Temperature, (double)cmd.HeatSetpoint, (double)cmd.CoolSetpoint, cmd.Mode, cmd.SystemStatus, cmd.Humidity);
            return thermostat;
        }

        private void ValidateTemputure(double? temputure)
        {
            if (temputure == null)
            {
                throw new ArgumentNullException("Invalid temputure specified: cannot be null.");
            }
        }

        private void ValidateHeatSetpoint(double? heatSetpoint)
        {
            if (heatSetpoint == null)
            {
                throw new ArgumentNullException("Invalid Heat Setpoint specified: cannot be null.");
            }
        }

        private void ValidateCoolSetpoint(double? coolSetpoint)
        {
            if (coolSetpoint == null)
            {
                throw new ArgumentNullException("Invalid Cool Setpoint specified: cannot be null.");
            }
        }

        private void ValidateMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new ArgumentNullException("Invalid Mode specified: cannot be null.");
            }
        }

        private void ValidateSystemStatus(string systemStatus)
        {
            if (string.IsNullOrWhiteSpace(systemStatus))
            {
                throw new ArgumentNullException("Invalid System Status specified: cannot be null.");
            }
        }
    }
}
