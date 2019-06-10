using NUnit.Framework;
using Moq;
using System.Reflection;
using System;
using System.IO;

using Should;
using System.Threading;

using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.ESFramework.Common.Base;
using Controller.Common.Commands;
using System.Linq;
using System.Collections.Generic;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_Connect()
        {
            DomoTrollerShare2.DomoShare domoShare = new DomoTrollerShare2.DomoShare();
            Assert.IsNotNull(domoShare);
            var connected = domoShare.Connected;
            Thread.Sleep(20000);
            domoShare.Connected.ShouldBeTrue();
        }
    }
}
