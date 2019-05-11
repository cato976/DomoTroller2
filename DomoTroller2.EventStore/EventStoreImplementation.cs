using System;
using System.Collections.Generic;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

using DomoTroller2.EventStore.Exceptions;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.ESFramework.Common.Base;

namespace DomoTroller2.EventStore
{
    public class EventStoreImplementation : IEventStore
    {
        private IEventStoreConnection _eventStoreConnection;
        private bool KeepReconnecting = true;

        private UserCredentials _creds;



        public void Connect(string connectionString, string user, string password, string certificateCommonName, bool useSsl = false, int reconnectAttempts = -1, int heartbeatInterval = 30, int heartbeatTimeout = 120)
        {
            var settings = ConnectionSettings
                .Create()
                .KeepReconnecting()
                .UseConsoleLogger();

            _creds = new UserCredentials(user, password);
            settings.SetDefaultUserCredentials(new UserCredentials(user, password));
            settings.SetHeartbeatInterval(TimeSpan.FromSeconds(heartbeatInterval));
            settings.SetHeartbeatTimeout(TimeSpan.FromSeconds(heartbeatTimeout));

            if (useSsl && !String.IsNullOrEmpty(certificateCommonName))

            {

                settings.UseSslConnection(certificateCommonName, true);
            }

            if (reconnectAttempts > 0)

            {

                settings.LimitReconnectionsTo(reconnectAttempts);
                KeepReconnecting = false;
            }

            var connectUri = new Uri(connectionString);
            _eventStoreConnection = EventStoreConnection.Create(settings, connectUri);
            _eventStoreConnection.ConnectAsync().Wait();
        }

        public void CreatePersistentSubscription(string eventGroupName, string eventStreamName, Dictionary<string, string> createValues = null)
        {
            throw new NotImplementedException();
        }

        public List<IEvent> GetAllEvents(CompositeAggregateId aggregateId)
        {
            throw new NotImplementedException();
        }

        public List<IEvent> GetAllEventsToEventIdInclusive(CompositeAggregateId aggregateId, string eventId)
        {
            throw new NotImplementedException();
        }

        public IEventStreamSubscription GetEventStreamPersistentSubscription(string eventGroupName, string eventStreamName)
        {
            throw new NotImplementedException();
        }

        public IEventStreamSubscription GetEventStreamSubscription(string streamName)
        {
            throw new NotImplementedException();
        }

        public void SaveEvent(IEvent @event, int expectedVersion)
        {
            throw new NotImplementedException();
        }

        public void SaveEvents(CompositeAggregateId aggregateId, IEnumerable<IEvent> events)
        {
            foreach (var @event in events)

            {

                AppendEventToEventStream(@event, aggregateId);

            }
        }

        private void AppendEventToEventStream(IEvent @event, CompositeAggregateId aggregateId)

        {

            var eventData = GetEventData(@event);

            try

            {

                TimeSpan timeout = KeepReconnecting ? TimeSpan.FromSeconds(30) : TimeSpan.FromMilliseconds(-1);

                _eventStoreConnection.AppendToStreamAsync(aggregateId.CompositeId, @event.Version, eventData).Wait(timeout);

            }

            catch(Exception)

            {

                throw new ConnectionFailure("Failed to persist event. There may be an issue with the connection to Event Store.");
            }

        }



        private static EventData GetEventData(IEvent @event)

        {

            return @event.SerializeEvent();
        }



    }
}
