using System;
using System.Collections.Generic;
using System.Reflection;
using DomoTroller2.Common.CommandBus;
using DomoTroller2.ESEvents.Common.Events.Unit;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using log4net;

namespace DomoTroller2.Api.Domain
{
    public class Unit : Aggregate
    {
        public Unit(Guid id, IEventMetadata eventMetadata, IEventStore eventStore)
        {
            ValidateUnitId(id);
            EventStore = eventStore;
            //ApplyEvent(new DoorOpened(id, DateTimeOffset.UtcNow, eventMetadata));
            //var events = this.GetUncommittedEvents();
            //SendEvent(new CompositeAggregateId(eventMetadata.TenantId, AggregateGuid, eventMetadata.Category), events);
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Device));
        private static IEventStore EventStore;

        public Unit DoorOpen(IEventMetadata eventMetadata, IEventStore eventStore, Commands.Unit.DoorOpenCommand cmd)
        {
            var doorOpenUnitCommand = new global::Unit.Common.Command.DoorOpen(cmd.UnitId);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(doorOpenUnitCommand);

            ApplyEvent(new DoorOpened(cmd.UnitId, DateTimeOffset.UtcNow, eventMetadata));
            var events = GetUncommittedEvents();
            SendEvent(new CompositeAggregateId(eventMetadata.TenantId, AggregateGuid, eventMetadata.Category), events);

            var unit = new Unit(cmd.UnitId, eventMetadata, eventStore);
            return unit;
        }

        public Unit DoorClose(IEventMetadata eventMetadata, IEventStore eventStore, Commands.Unit.DoorCloseCommand cmd)
        {
            var doorCloseUnitCommand = new global::Unit.Common.Command.DoorClose(cmd.UnitId);
            var commandBus = CommandBus.Instance;
            commandBus.Execute(doorCloseUnitCommand);

            ApplyEvent(new DoorOpened(cmd.UnitId, DateTimeOffset.UtcNow, eventMetadata));
            var events = GetUncommittedEvents();
            SendEvent(new CompositeAggregateId(eventMetadata.TenantId, AggregateGuid, eventMetadata.Category), events);

            var unit = new Unit(cmd.UnitId, eventMetadata, eventStore);
            return unit;
        }

        private void Apply(DoorOpened e)
        {
            AggregateGuid = e.AggregateGuid;
        }

        private void Apply(DoorClosed e)
        {
            AggregateGuid = e.AggregateGuid;
        }

        private void ValidateUnitId(Guid unitId)
        {
            if(unitId == Guid.Empty)
            {
                throw new ArgumentException("Invalid unit Id specified: cannot be default value.", "unit Id");
            }
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
