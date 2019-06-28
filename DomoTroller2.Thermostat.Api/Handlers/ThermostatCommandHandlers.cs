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
            ValidateHeatSetpoint(message.NewHeatSetpoint);

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
            ValidateHeatSetpoint(message.NewCoolSetpoint);

            //Process
            var tenantId = message.TenantId;
            var eventMetadata = new EventMetadata(tenantId, "Thermostat", Guid.NewGuid().ToString(), Guid.NewGuid(), tenantId, DateTimeOffset.UtcNow);
            var thermostat = new Domain.Thermostat(message.Id);

            Repository<Domain.Thermostat> thermostatRepo = new Repository<Domain.Thermostat>(message.EventStore);

            CoolSetpointChangeCommand cmd = new CoolSetpointChangeCommand(tenantId, message.ThermostatId, message.Id, message.NewCoolSetpoint);

            var found = thermostatRepo.GetById(new CompositeAggregateId(tenantId, message.Id, "Thermostat"));

            found.ChangeCoolSetpoint(eventMetadata, message.EventStore, cmd, ((Aggregate)found).EventMetadata.EventNumber);
        }

        private static void ValidateHeatSetpoint(double? heatSetpoint)
        {
            if (heatSetpoint == null)
            {
                throw new ArgumentNullException("Invalid Heat Setpoint specified: cannot be null.");
            }
        }

        private void Apply(CoolSetpointChanged e)
        {
            HeatSetpoint = e.NewCoolSetpoint;
        }
    }
}
