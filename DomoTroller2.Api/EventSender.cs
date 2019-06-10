using System;
using System.Collections.Generic;
using System.Reflection;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.EventStore.Exceptions;
using log4net;

namespace DomoTroller2.Api
{
    public static class EventSender
    {
        private static IEventStore EventStore;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventSender));

        public static bool SendEvent(IEventStore eventStore, CompositeAggregateId compositeId, IEnumerable<Event> events)
        {
            EventStore = eventStore;

            try
            {
                EventStore.SaveEvents(compositeId, events);
                return true;
            }
            catch (ConnectionFailure conn)
            {
                Logger.Error($"Exception: {conn.Message} in {Assembly.GetExecutingAssembly().GetName()}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception: {ex.Message} in {Assembly.GetExecutingAssembly().GetName()}");
                throw;
            }
        }

    }
}
