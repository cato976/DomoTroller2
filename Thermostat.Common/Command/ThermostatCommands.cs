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

    public class ChangeAmbientTemperature : ICommand
    {
        public ChangeAmbientTemperature(IEventStore eventStore, string thermostatId, Guid id, Guid tenantId, double? newAmbientTemperature)
        {
            ThermostatId = thermostatId;
            Id = id;
            TenantId = tenantId;
            NewAmbientTemperature = newAmbientTemperature;
            EventStore = eventStore;
        }

        public string ThermostatId { get; private set; }
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public double? NewAmbientTemperature { get; private set; }
        public  IEventStore EventStore { get; private set; }
        
    }

    public class ChangeHumidity : ICommand
    {
        public ChangeHumidity(IEventStore eventStore, string thermostatId, Guid id, Guid tenantId, double? newHumidity)
        {
            ThermostatId = thermostatId;
            Id = id;
            TenantId = tenantId;
            NewHumidity = newHumidity;
            EventStore = eventStore;
        }

        public string ThermostatId { get; private set; }
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public double? NewHumidity { get; private set; }
        public  IEventStore EventStore { get; private set; }
        
    }

    public class ChangeSystemStatus : ICommand
    {
        public ChangeSystemStatus(IEventStore eventStore, string thermostatId, Guid id, Guid tenantId, string newSystemStatus)
        {
            ThermostatId = thermostatId;
            Id = id;
            TenantId = tenantId;
            NewSystemState = newSystemStatus;
            EventStore = eventStore;
        }

        public string ThermostatId { get; private set; }
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public string NewSystemState { get; private set; }
        public  IEventStore EventStore { get; private set; }
        
    }

    public class ChangeSystemMode : ICommand
    {
        public ChangeSystemMode(IEventStore eventStore, string thermostatId, Guid id, Guid tenantId, string newSystemMode)
        {
            ThermostatId = thermostatId;
            Id = id;
            TenantId = tenantId;
            NewSystemMode = newSystemMode;
            EventStore = eventStore;
        }

        public string ThermostatId { get; private set; }
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public string NewSystemMode { get; private set; }
        public  IEventStore EventStore { get; private set; }
        
    }
}
