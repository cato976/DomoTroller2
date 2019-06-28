using System;
using System.Collections.Generic;
using DomoTroller2.ESFramework.Common.Base;
using log4net;
using System.Reflection;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.Common
{
    public static class EventSender
    {
        private static IEventStore EventStore;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IAggregate));

        #region IEventSender

        public static bool SendEvent(IEventStore eventStore, CompositeAggregateId compositeId, IEnumerable<Event> events)
        {
            EventStore = eventStore;

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

        #endregion IEventSender
    }
}
