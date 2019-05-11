using System;
using System.Text;
using DomoTroller2.ESFramework.Common.Base;
using DomoTroller2.ESFramework.Common.Interfaces;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DomoTroller2.EventStore
{
    public static class EventSerialization
    {
        public static string EventClrTypeHeader = "EventClrTypeName";

        public static EventData SerializeEvent(this IEvent serializableEvent)
        {
            if (serializableEvent == null)
            {
                throw new ArgumentNullException(nameof(serializableEvent));
            } 

            if (string.IsNullOrEmpty(serializableEvent.Metadata.EventId))
            {
                serializableEvent.Metadata.EventId =  Guid.NewGuid().ToString();
            }

            serializableEvent.Metadata.CustomMetadata[EventClrTypeHeader] = serializableEvent.GetType().AssemblyQualifiedName; 
            
            var json = JsonConvert.SerializeObject(serializableEvent, Formatting.Indented);
            var data = Encoding.UTF8.GetBytes(json);
            var eventName = serializableEvent.GetType().Name;
            return new EventData(Guid.Parse(serializableEvent.Metadata.EventId), eventName, true, data,null);
        }

        public static IEvent DeserializeEvent(this RecordedEvent orginalEvent)

        {

            var eventDataJson = Encoding.UTF8.GetString(orginalEvent.Data);

            if (string.IsNullOrWhiteSpace(eventDataJson) || eventDataJson == "{}")

            {

                throw new Exception("eventDataJson is empty");

            }



            var parsedObject = JObject.Parse(eventDataJson);

            var metaDataJToken = parsedObject["Metadata"];

            if (metaDataJToken == null)

            {

                throw new Exception("metaDataJToken is null");

            }



            var eventMetadata = metaDataJToken.ToObject<EventMetadata>();

            var eventData = DeserializeObject(eventDataJson, eventMetadata.CustomMetadata[EventClrTypeHeader]) as IEvent;

            if (eventData == null)

            {

                throw new Exception("eventData is null");

            }



            eventData.Metadata.EventNumber = orginalEvent.EventNumber;

            eventData.Metadata.EventId = orginalEvent.EventId.ToString();



            return eventData;

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

                return null;
            }
        }
    }
}
