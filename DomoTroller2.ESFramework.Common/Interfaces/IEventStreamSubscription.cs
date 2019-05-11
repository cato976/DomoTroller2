using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomoTroller2.ESFramework.Common.Exceptions;

namespace DomoTroller2.ESFramework.Common.Interfaces
{
    public interface IEventStreamSubscription
    {
        event Func<IEvent, Task> EventArrived;
        event Action<IEventStreamSubscription, EventStreamSubscriptionException> EventStreamSubscriptionDropped;
        string EventStreamName { get; set; }
        int InFlightEvents { get; set; }
        long MessageTimeout { get; set; }
        void Start(Dictionary<string, string> con = null);
        void Stop();
        void UpdateSubscription(Dictionary<string, string> keyValuePairs);
        void DeleteSubscription(Dictionary<string, string> keyValuePairs);
    }
}
