using DomoTroller2.ESEvents.Common.Events.Thermostat;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.EventStore;
using DomoTroller2.Thermostat.Api.Commands;
using System;
using System.Linq;
using Thermostat.Common.Command;

namespace DomoTroller2.Thermostat.Api.Handlers
{
    public class ThermostatCommandHandlers
    {
        public ThermostatCommandHandlers() { }

        public double HeatSetpoint { get; private set; }

        public void Handle(ConnectThermostat message)
        {
            //Validate
            //Process
        }

        public void Handle(ChangeHeatSetpoint message)
        {
            //Validate
            ValidateSetpoint(message.NewHeatSetpoint);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);
            var thermostat = new Domain.Thermostat(message.Id);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            HeatSetpointChangeCommand cmd = new HeatSetpointChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewHeatSetpoint);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeHeatSetpoint(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        public void Handle(ChangeCoolSetpoint message)
        {
            //Validate
            ValidateSetpoint(message.NewCoolSetpoint);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            CoolSetpointChangeCommand cmd = new CoolSetpointChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewCoolSetpoint);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeCoolSetpoint(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        public void Handle(ChangeAmbientTemperature message)
        {
            //Validate
            ValidateAmbientTemperature(message.NewAmbientTemperature);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            AmbientTemperatureChangeCommand cmd = new AmbientTemperatureChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewAmbientTemperature);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeAmbientTemperature(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        public void Handle(ChangeHumidity message)
        {
            //Validate
            ValidateHumidity(message.NewHumidity);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            HumidityChangeCommand cmd = new HumidityChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewHumidity);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeHumidity(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        public void Handle(ChangeSystemStatus message)
        {
            //Validate
            ValidateSystemStatus(message.NewSystemState);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            SystemStatusChangeCommand cmd = new SystemStatusChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewSystemState);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeSystemState(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        public void Handle(ChangeSystemMode message)
        {
            //Validate
            ValidateSystemMode(message.NewSystemMode);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            SystemModeChangeCommand cmd = new SystemModeChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewSystemMode);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeSystemMode(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        private static void ValidateSetpoint(double? setpoint)
        {
            if (setpoint == null)
            {
                throw new ArgumentNullException("Invalid Setpoint specified: cannot be null.");
            }
        }

        private static void ValidateAmbientTemperature(double? ambientTemperature)
        {
            if (ambientTemperature == null)
            {
                throw new ArgumentNullException("Invalid ambient temperature specified: cannot be null.");
            }
        }

        private static void ValidateHumidity(double? humidity)
        {
            if (humidity == null)
            {
                throw new ArgumentNullException("Invalid humidity specified: cannot be null.");
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

        private void Apply(CoolSetpointChanged e)
        {
            HeatSetpoint = e.NewCoolSetpoint;
        }
    }
}
