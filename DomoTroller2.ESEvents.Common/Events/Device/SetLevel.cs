using System;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.ESEvents.Common.Events.Device
{
    public class SetLevel : Event
    {
        public SetLevel(Guid aggregateGuid, DateTimeOffset effectiveDateTime, IEventMetadata metadata, int percentage) : base(aggregateGuid, effectiveDateTime, metadata)
        {
            Id = aggregateGuid;
            Percentage = percentage;
        }

        public Guid Id { get; private set; }
        public int Percentage { get; private set; }
    }
}
