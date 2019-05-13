using System;
using System.Collections.Generic;
using System.Reflection;
using Device.Common.Command;
using DomoTroller2.Api.Commands.Device;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using log4net;

namespace DomoTroller2.Api.Domain
{
    public class Device : Aggregate
    {
        private Device(Guid id, IEventMetadata eventMetadata, IEventStore eventStore, int level)
        {
            ValidateDeviceId(id);
            level = ValidateLevel(level);
            EventStore = eventStore;
            ApplyEvent(new TurnedOn(id, DateTimeOffset.UtcNow, eventMetadata, level));
            var events = this.GetUncommittedEvents();
            SendEvent(new CompositeAggregateId(eventMetadata.TenantId, AggregateGuid, eventMetadata.Category), events);
        }

        public int Level { get; private set; }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Device));
        private static IEventStore EventStore;

        public static Device TurnOn(IEventMetadata eventMetadata, IEventStore eventStore, Commands.Device.TurnOnCommand cmd)
        {
            var turnOnDeviceCommand = new global::Device.Common.Command.TurnOn(cmd.DeviceId, cmd.Level);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(turnOnDeviceCommand);
            var device = new Device(cmd.DeviceId, eventMetadata, eventStore, cmd.Level);
            return device;
        }

        public void SetLevel(IEventMetadata eventMetadata, Guid id, int level, int originalVersion)
        {
            ValidateDeviceId(id);
            ValidateVersion(originalVersion);
            level = ValidateLevel(level);
            ApplyEvent(new ESEvents.Common.Events.Device.SetLevel(AggregateGuid, DateTimeOffset.UtcNow, eventMetadata, level));
            var events = this.GetUncommittedEvents();
            SendEvent(new CompositeAggregateId(eventMetadata.TenantId, AggregateGuid, eventMetadata.Category), events);
        }

        private void Apply(TurnedOn e)
        {
            AggregateGuid = e.AggregateGuid;
            Level = e.Level;
        }

        private void ValidateDeviceId(Guid deviceId)
        {
            if(deviceId == Guid.Empty)
            {
                throw new ArgumentException("Invalid device Id specified: cannot be default value.", "device Id");
            }
        }

        private int ValidateLevel(int level)
        {
            if (level < 0)
            {
                throw new ArgumentException("Invalid device level specified: cannot be less than 0.", "level");
            }
            else if (level > 100)
            {
                return 100;
            }

            return level;
        }

        protected void ValidateVersion(int version)
        {
            if (Version != version)
            {
                throw new ArgumentOutOfRangeException("version", "Invalid version specified: the version is out of sync.");
            }
        }

        private bool SendEvent(CompositeAggregateId compositeId, IEnumerable<Event> events)
        {
            try
            {
                EventStore.SaveEvents(compositeId, events);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception: {ex.Message} in {Assembly.GetExecutingAssembly().GetName().Name}");
                throw;
            }
        }
    }
}
