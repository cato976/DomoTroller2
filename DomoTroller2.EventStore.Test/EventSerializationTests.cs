using System;
using NUnit.Framework;

using DomoTroller2.EventStore;
using DomoTroller2.ESFramework.Common.Base;
using System.Text;
using Newtonsoft.Json.Linq;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;
using DomoTroller2.ESEvents.Common.Events.Device;
using DomoTroller2.ESEvents.Common.Events.Controller;
using DomoTroller2.ESEvents.Common.Events.Unit;

namespace DomoTroller2.EventStore.Test
{
    public class EventSerializationTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public static string EventClrTypeHeader = "EventClrTypeName";

        [Test]
        public void EventSerialization_CreateEventMetadata_InvalidTenantId_ShouldThrow_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new EventMetadata(new Guid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        }

        [Test]
        public void EventSerialization_CreateEventMetadata_InvalidCategory_ShouldThrow_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EventMetadata(Guid.NewGuid(), null, "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        }

        [Test]
        public void EventSerialization_CreateEventMetadata_InvalidCorrelationId_ShouldThrow_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EventMetadata(Guid.NewGuid(), "testCategory", null, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        }

        [Test]
        public void EventSerialization_CreateEventMetadata_InvalidPublishedDateTime_ShouldThrow_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new EventMetadata(Guid.NewGuid(), "testCategory", "testCorrelationId", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.MinValue));
        }

        [Test]
        public void EventSerialization_SerializeDeviceTurnedOnEvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new TurnedOn(Guid.NewGuid(), new DateTimeOffset(DateTime.UtcNow), eventMetaData, 100);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            TurnedOn castDeserializedEvent = (TurnedOn) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeControllerConnectedEvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new Connected(Guid.NewGuid(), new DateTimeOffset(DateTime.UtcNow), eventMetaData);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            Connected castDeserializedEvent = (Connected) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeThermostatConnectedEvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new ESEvents.Common.Events.Thermostat.Connected(Guid.NewGuid(), 
                new DateTimeOffset(DateTime.UtcNow), eventMetaData, 76, 55, 77, "Cool", "Idel", 45);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            ESEvents.Common.Events.Thermostat.Connected castDeserializedEvent = (ESEvents.Common.Events.Thermostat.Connected) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeThermostatHeatPointChangedEvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new ESEvents.Common.Events.Thermostat.CoolSetpointChanged(Guid.NewGuid(), 
                new DateTimeOffset(DateTime.UtcNow), eventMetaData, 55);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            ESEvents.Common.Events.Thermostat.CoolSetpointChanged castDeserializedEvent = (ESEvents.Common.Events.Thermostat.CoolSetpointChanged) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeDeviceSetLevelEvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new SetLevel(Guid.NewGuid(), new DateTimeOffset(DateTime.UtcNow), eventMetaData, 100);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            SetLevel castDeserializedEvent = (SetLevel) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeDeviceDoorOpenedEventvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new DoorOpened(Guid.NewGuid(), new DateTimeOffset(DateTime.UtcNow), eventMetaData);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            DoorOpened castDeserializedEvent = (DoorOpened) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeDeviceDoorClosedEventvent_ShouldDeserialize()
        {
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var serializableEvent = new DoorClosed(Guid.NewGuid(), new DateTimeOffset(DateTime.UtcNow), eventMetaData);

            var eventData = EventSerialization.SerializeEvent(serializableEvent);

            var eventDataJson = Encoding.UTF8.GetString(eventData.Data);
            Console.WriteLine(eventDataJson);
            var parsedJObject = JObject.Parse(eventDataJson);
            var metaDataJToken = parsedJObject["Metadata"];

            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();
            var deserializedEventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;
            DoorClosed castDeserializedEvent = (DoorClosed) deserializedEventData;

            Assert.IsNotNull(deserializedEventData);
            Assert.AreEqual(serializableEvent.Metadata.AccountGuid, deserializedEventData.Metadata.AccountGuid);
            Assert.AreEqual(serializableEvent.AggregateGuid, deserializedEventData.AggregateGuid);
            Assert.AreEqual(serializableEvent.EffectiveDateTime, deserializedEventData.EffectiveDateTime);
        }

        [Test]
        public void EventSerialization_SerializeTurnedOnEvent_ShouldThrow_ArgumentException()
        {
            DateTime date = new DateTime();
            var eventMetaData = new EventMetadata(Guid.NewGuid(), "testCat", "testCor", Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            Assert.Throws<ArgumentException>(() => new TurnedOn(Guid.NewGuid(), date, eventMetaData, 55));
        }

        [Test]
        public void EventSerialization_SerializeTurnedOnEvent_ShouldThrow_ArgumentNullException()
        {
            DateTime date = DateTime.UtcNow;
            Assert.Throws<ArgumentNullException>(() => new TurnedOn(Guid.NewGuid(), date, null, 7));
        }

        private static object DeserializeObject(string eventDataJson, string typeName)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(eventDataJson, Type.GetType(typeName));
                return obj;
            }
            catch (JsonReaderException)
            {
                throw;
            }
        }
    }
}
