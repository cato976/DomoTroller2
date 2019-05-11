using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.ESFramework.Common.Base
{
    public class EventMetadata : IEventMetadata
    {
        public EventMetadata()
        {

        }

        public EventMetadata(Guid tenantId, string category, string correlationId, Guid causationId,
            Guid accountGuid, DateTimeOffset publishedDateTime)
        {
            if (Guid.Empty == tenantId)
            {
                throw new ArgumentException(nameof(tenantId));
            }
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentNullException(nameof(category));
            }
            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentNullException(nameof(correlationId));
            }
            if (Guid.Empty == accountGuid)
            {
                throw new ArgumentException(nameof(accountGuid));
            }
            if (DateTimeOffset.MinValue == publishedDateTime)
            {
                throw new ArgumentException(nameof(publishedDateTime));
            }

            TenantId = tenantId;
            Category = category;
            CorrelationId = correlationId;
            CausationId = causationId;
            AccountGuid = accountGuid;
            PublishedDateTime = publishedDateTime;
        }

        public Guid TenantId { get; set; }
        public string Category { get; set; }
        public string CorrelationId { get; set; }
        public Guid CausationId { get; set; }
        public Guid AccountGuid { get; set; }
        public DateTimeOffset PublishedDateTime { get; set; }
        [JsonIgnore]
        public long EventNumber { get; set; }
        public string EventId { get; set; }
        public Dictionary<string, string> CustomMetadata { get; } = new Dictionary<string, string>();
    }
}
