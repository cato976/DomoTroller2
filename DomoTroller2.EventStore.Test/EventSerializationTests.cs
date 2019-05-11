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
    }
}
