using System;

namespace DomoTroller2.ESFramework.Common.Exceptions
{
    public class EventStreamSubscriptionException : Exception
    {
        public EventStreamSubscriptionException(string subscriptionDropReason, Exception exception) : base(subscriptionDropReason, exception)
        {
        }
    }
}
