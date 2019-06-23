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
        public Thermostat()
        {
            Register<Connected>(OnConnected);
            Register<CoolSetpointChanged>(OnHeatSetpointChanged);
        }

        public Thermostat(Guid thermostatGuid)
        {
            AggregateGuid = thermostatGuid;
        }

        private Thermostat(IEventMetadata eventMetadata, IEventStore eventStore)
        {
            EventMetadata = eventMetadata;
            EventMetadata.Category = "Thermostat";
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

        public double HeatSetpoint { get; private set; }

        private new readonly IEventMetadata EventMetadata;
        private readonly IEventStore EventStore;

        public static Thermostat ConnectToThermostat(IEventMetadata eventMetadata, IEventStore eventStore, ConnectToThermostatCommand cmd)
        {
            ValidateTemputure(cmd.Temperature);
            ValidateHeatSetpoint(cmd.HeatSetpoint);
            ValidateCoolSetpoint(cmd.CoolSetpoint);
            ValidateMode(cmd.Mode);
            ValidateSystemStatus(cmd.SystemStatus);
            ValidateTenantId(cmd.TenantId);

            var thermostatId = cmd.ThermostatId;
            var connectToControllerCommand = new ConnectThermostat(cmd.ThermostatAggregateId);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(connectToControllerCommand);

            var thermostat = new Thermostat(cmd.ThermostatAggregateId, eventMetadata, eventStore, 
                (double)cmd.Temperature, (double)cmd.HeatSetpoint, (double)cmd.CoolSetpoint, 
                cmd.Mode, cmd.SystemStatus, cmd.Humidity);

            Repository<Thermostat> thermostatRepo = new Repository<Thermostat>(eventStore);
            var found = thermostatRepo.GetById(new CompositeAggregateId(cmd.TenantId, cmd.ThermostatAggregateId, "Thermostat"));
            return found;
        }

        public void ChangeHeatSetpoint(IEventMetadata eventMetadata, IEventStore eventStore,
            HeatSetpointChangeCommand cmd, long orginalEventNumber)
        {
            ValidateHeatSetpoint(cmd.NewHeatSetpoint);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var connectToControllerCommand = new ChangeHeatSetpoint(eventStore, cmd.ThermostatId, 
                cmd.ThermostatGuid, cmd.TenantId, (double)cmd.NewHeatSetpoint);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(connectToControllerCommand);

            ApplyEvent(new HeatSetpointChanged(cmd.ThermostatGuid, DateTimeOffset.UtcNow, eventMetadata, (double)cmd.NewHeatSetpoint), -4);

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

        public void ChangeCoolSetpoint(IEventMetadata eventMetadata, IEventStore eventStore,
            CoolSetpointChangeCommand cmd, long orginalEventNumber)
        {
            ValidateHeatSetpoint(cmd.NewCoolSetpoint);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var connectToControllerCommand = new ChangeCoolSetpoint(eventStore, cmd.ThermostatId, 
                cmd.ThermostatGuid, cmd.TenantId, (double)cmd.NewCoolSetpoint);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(connectToControllerCommand);

            ApplyEvent(new CoolSetpointChanged(cmd.ThermostatGuid, DateTimeOffset.UtcNow, eventMetadata, (double)cmd.NewCoolSetpoint), -4);

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

        private static void ValidateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentNullException("Invalid Category specified: cannot be null or empty.");
            }
            else if (!string.Equals(category.ToLowerInvariant(), "thermostat"))
            {
                throw new ArgumentNullException("Invalid Category specified: must be Thermostat.");
            }
        }

        private static void ValidateTemputure(double? temputure)
        {
            if (temputure == null)
            {
                throw new ArgumentNullException("Invalid temputure specified: cannot be null.");
            }
        }

        private static void ValidateHeatSetpoint(double? heatSetpoint)
        {
            if (heatSetpoint == null)
            {
                throw new ArgumentNullException("Invalid Heat Setpoint specified: cannot be null.");
            }
        }

        private static void ValidateCoolSetpoint(double? coolSetpoint)
        {
            if (coolSetpoint == null)
            {
                throw new ArgumentNullException("Invalid Cool Setpoint specified: cannot be null.");
            }
        }

        private static void ValidateMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new ArgumentNullException("Invalid Mode specified: cannot be null or empty.");
            }
        }

        private static void ValidateSystemStatus(string systemStatus)
        {
            if (string.IsNullOrWhiteSpace(systemStatus))
            {
                throw new ArgumentNullException("Invalid System Status specified: cannot be null or empty.");
            }
        }

        private static void ValidateTenantId(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException("Invalid Tenant Id specified: cannot be null or empty.");
            }
        }

        protected void ValidateEventNumber(long eventNumber)
        {
            if (((Aggregate)this).EventMetadata.EventNumber != eventNumber)
            {
                throw new ArgumentOutOfRangeException("version", "Invalid event number specified: the event number is out of sync.");
            }
        }

        private void Apply(Connected e)
        {
            AggregateGuid = e.AggregateGuid;
            HeatSetpoint = e.HeatSetpoint;
        }

        private void Apply(CoolSetpointChanged e)
        {
            HeatSetpoint = e.NewCoolSetpoint;
        }

        private void OnHeatSetpointChanged(CoolSetpointChanged heatSetpointChanged)
        {
            AggregateGuid = heatSetpointChanged.AggregateGuid;
            HeatSetpoint = heatSetpointChanged.NewCoolSetpoint;
        }

        private void OnConnected(Connected connected)
        {
            AggregateGuid = connected.AggregateGuid;
            HeatSetpoint = connected.HeatSetpoint;
        }
    }
}
