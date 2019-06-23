using System;

namespace DomoTroller2.EventStore.Exceptions
{
    public class StreamNotFoundException : Exception
    {
        public StreamNotFoundException(string streamName) : base(streamName) { }
    }
}
