using System;

namespace DomoTroller2.EventStore.Exceptions
{
    public class StreamDeletedException : Exception
    {
        public StreamDeletedException(string streamName) : base(streamName) { }
    }
}
