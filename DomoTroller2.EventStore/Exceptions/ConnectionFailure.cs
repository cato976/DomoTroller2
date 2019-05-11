using System;

namespace DomoTroller2.EventStore.Exceptions
{
    public class ConnectionFailure : Exception
    {
        public ConnectionFailure(string message) : base(message)
        {
            
        }
    }
}
