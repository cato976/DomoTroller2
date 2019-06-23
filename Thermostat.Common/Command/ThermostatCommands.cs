using DomoTroller2.Common;
using DomoTroller2.ESFramework.Common.Interfaces;
using System;

namespace Thermostat.Common.Command
{
    public class ConnectThermostat : ICommand
    {
        public ConnectThermostat(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }

    public class ChangeHeatSetpoint : ICommand
    {
        public ChangeHeatSetpoint(IEventStore eventStore, string thermostatId, Guid id, Guid tenantId, double? newHeatSetpoint)
        {
            ThermostatId = thermostatId;
            Id = id;
            TenantId = tenantId;
            NewHeatSetpoint = newHeatSetpoint;
            EventStore = eventStore;
        }

        public string ThermostatId { get; private set; }
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public double? NewHeatSetpoint { get; private set; }
        public  IEventStore EventStore { get; private set; }
    }

    public class ChangeCoolSetpoint : ICommand
    {
        public ChangeCoolSetpoint(IEventStore eventStore, string thermostatId, Guid id, Guid tenantId, double? newCoolSetpoint)
        {
            ThermostatId = thermostatId;
            Id = id;
            TenantId = tenantId;
            NewCoolSetpoint = newCoolSetpoint;
            EventStore = eventStore;
        }

        public string ThermostatId { get; private set; }
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public double? NewCoolSetpoint { get; private set; }
        public  IEventStore EventStore { get; private set; }
    }
}
