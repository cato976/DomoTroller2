using System;
using NUnit.Framework;

using DomoTroller2.ESFramework.Common.Base;

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
    }
}
