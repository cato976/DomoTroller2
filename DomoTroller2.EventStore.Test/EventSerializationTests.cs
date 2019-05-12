using System;
using NUnit.Framework;

using DomoTroller2.EventStore;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESEvents.Common.Events.Device;
using System.Text;
using Newtonsoft.Json.Linq;
using DomoTroller2.ESFramework.Common.Interfaces;
using Newtonsoft.Json;

namespace Tests
{
    public class Tests
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
        public void EventSerialization_SerializeDeviceTurnOnEvent_ShouldDeserialize()
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
