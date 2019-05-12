using System;
using System.Collections.Generic;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.ESFramework.Common.Base
{
    public abstract class Aggregate : IAggregate
    {
        internal readonly List<Event> events = new List<Event>();

        public Guid AggregateGuid { get; protected set; }

        private int version = -3;
        public int Version { get { return version; } internal set { version = value; } }

        public IEnumerable<Event> GetUncommittedEvents()
        {
            return events;
        }

        public void MarkEventsAsCommitted()
        {
            events.Clear();
        }

        public void LoadStateFromHistory(IEnumerable<Event> history)
        {
            foreach (var e in history) ApplyEvent(e, false);
        }

        protected internal void ApplyEvent(Event @event)
        {
            ApplyEvent(@event, true);
        }

        protected virtual void ApplyEvent(Event @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew)
            {
                @event.Version = ++Version;
                events.Add(@event);
            }
            else
            {
                Version = @event.Version;
            }
        }
    }
}
