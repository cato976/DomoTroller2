using DomoTroller2.Common;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.ESEvents.Common.Events.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.EventStore.Exceptions;
using DomoTroller2.Thermostat.Api.Commands;
using System;
using System.Diagnostics;
using Thermostat.Common.Command;

namespace DomoTroller2.Thermostat.Api.Domain
{
    public class Thermostat : Aggregate
    {
        public Thermostat()
        {
            Register<Connected>(OnConnected);
            Register<CoolSetpointChanged>(OnCoolSetpointChanged);
            Register<HeatSetpointChanged>(OnHeatSetpointChanged);
            Register<AmbientTemperatureChanged>(OnAmbientTemperatureChanged);
            Register<HumidityChanged>(OnHumidityChanged);
            Register<SystemStatusChanged>(OnStateChanged);
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
        public double CoolSetpoint { get; private set; }
        public double AmbientTemperature { get; private set; }
        public double? Humidity { get; private set; }
        public string SystemStatus { get; private set; }

        private new readonly IEventMetadata EventMetadata;
        private readonly IEventStore EventStore;

        public static Thermostat ConnectToThermostat(IEventMetadata eventMetadata, IEventStore eventStore, ConnectToThermostatCommand cmd)
        {
            ValidateTemperature(cmd.Temperature);
            ValidateSetpoint(cmd.HeatSetpoint);
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
            ValidateSetpoint(cmd.NewHeatSetpoint);
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
            ValidateSetpoint(cmd.NewCoolSetpoint);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var coolSetpointChangeCommand = new ChangeCoolSetpoint(eventStore, cmd.ThermostatId, 
                cmd.ThermostatGuid, cmd.TenantId, (double)cmd.NewCoolSetpoint);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(coolSetpointChangeCommand);

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

        public void ChangeAmbientTemperature(IEventMetadata eventMetadata, IEventStore eventStore,
            AmbientTemperatureChangeCommand cmd, long orginalEventNumber)
        {
            ValidateTemperature(cmd.NewAmbientTemperature);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var changeAmbientTemperatureCommand = new ChangeAmbientTemperature(eventStore, cmd.ThermostatId,
                cmd.ThermostatGuid, cmd.TenantId, (double)cmd.NewAmbientTemperature);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(changeAmbientTemperatureCommand);

            ApplyEvent(new AmbientTemperatureChanged(cmd.ThermostatGuid, DateTimeOffset.UtcNow, eventMetadata,
                (double)cmd.NewAmbientTemperature), -4);

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

        public void ChangeHumidity(IEventMetadata eventMetadata, IEventStore eventStore,
            HumidityChangeCommand cmd, long orginalEventNumber)
        {
            ValidateHumidity(cmd.NewHumidity);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var changeHumidityCommand = new ChangeHumidity(eventStore, cmd.ThermostatId,
                cmd.ThermostatGuid, cmd.TenantId, (double)cmd.NewHumidity);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(changeHumidityCommand);

            ApplyEvent(new HumidityChanged(cmd.ThermostatGuid, DateTimeOffset.UtcNow, eventMetadata,
                (double)cmd.NewHumidity), -4);

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

        public void ChangeSystemState(IEventMetadata eventMetadata, IEventStore eventStore,
            SystemStatusChangeCommand cmd, long orginalEventNumber)
        {
            ValidateSystemStatus(cmd.NewSystemStatus);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var changeSystemStatusCommand = new ChangeSystemStatus(eventStore, cmd.ThermostatId,
                cmd.ThermostatGuid, cmd.TenantId, cmd.NewSystemStatus);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(changeSystemStatusCommand);

            ApplyEvent(new SystemStatusChanged(cmd.ThermostatGuid, DateTimeOffset.UtcNow, eventMetadata,
                cmd.NewSystemStatus), -4);

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

        public void ChangeSystemMode(IEventMetadata eventMetadata, IEventStore eventStore,
            SystemModeChangeCommand cmd, long orginalEventNumber)
        {
            ValidateSystemMode(cmd.NewSystemMode);
            ValidateEventNumber(orginalEventNumber);
            ValidateCategory(eventMetadata.Category);

            var changeSystemModeCommand = new ChangeSystemMode(eventStore, cmd.ThermostatId,
                cmd.ThermostatGuid, cmd.TenantId, cmd.NewSystemMode);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(changeSystemModeCommand);

            ApplyEvent(new SystemModeChanged(cmd.ThermostatGuid, DateTimeOffset.UtcNow, eventMetadata,
                cmd.NewSystemMode), -4);

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

        private static void ValidateTemperature(double? temputure)
        {
            if (temputure == null)
            {
                throw new ArgumentNullException("Invalid temputure specified: cannot be null.");
            }
        }

        private static void ValidateSetpoint(double? heatSetpoint)
        {
            if (heatSetpoint == null)
            {
                throw new ArgumentNullException("Invalid Setpoint specified: cannot be null.");
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

        private static void ValidateSystemMode(string systemMode)
        {
            if (string.IsNullOrWhiteSpace(systemMode))
            {
                throw new ArgumentNullException("Invalid System Mode specified: cannot be null or empty.");
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

        private static void ValidateHumidity(double? humidity)
        {
            if (humidity == null)
            {
                throw new ArgumentNullException("Invalid humidity temperature specified: cannot be null.");
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

        private void OnHeatSetpointChanged(HeatSetpointChanged heatSetpointChanged)
        {
            AggregateGuid = heatSetpointChanged.AggregateGuid;
            HeatSetpoint = heatSetpointChanged.NewHeatSetpoint;
        }

        private void OnCoolSetpointChanged(CoolSetpointChanged heatSetpointChanged)
        {
            AggregateGuid = heatSetpointChanged.AggregateGuid;
            CoolSetpoint = heatSetpointChanged.NewCoolSetpoint;
        }

        private void OnAmbientTemperatureChanged(AmbientTemperatureChanged ambientTemperatureChanged)
        {
            AggregateGuid = ambientTemperatureChanged.AggregateGuid;
            AmbientTemperature = ambientTemperatureChanged.NewAmbientTemperature;
        }

        private void OnHumidityChanged(HumidityChanged humidityChanged)
        {
            AggregateGuid = humidityChanged.AggregateGuid;
            Humidity = humidityChanged.NewHumidity;
        }

        private void OnStateChanged(SystemStatusChanged stateChanged)
        {
            AggregateGuid = stateChanged.AggregateGuid;
            SystemStatus = stateChanged.NewSystemStatus;
        }

        private void OnConnected(Connected connected)
        {
            AggregateGuid = connected.AggregateGuid;
            HeatSetpoint = connected.HeatSetpoint;
            CoolSetpoint = connected.CoolSetpoint;
            AmbientTemperature = connected.Temperature;
            Humidity = connected.Humidity;
            SystemStatus = connected.SystemStatus;
        }
    }
}
