using NUnit.Framework;
using Moq;
using System.Reflection;
using System;
using System.IO;

using Should;
using System.Threading;

using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.ESFramework.Common.Base;
using Controller.Common.Command;
using System.Linq;
using System.Collections.Generic;
using DomoTrollerShare2.Interfaces;
using System.Diagnostics;
using HAI_Shared;
using DomoTrollerShare2;
using DomoTrollerShare2.EventArguments;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        //[Test]
        //public void Test_Connect()
        //{
        //    var moqHAC = new Mock<IHAC>();
        //    DomoShare domoShare = new DomoShare();
        //    Assert.IsNotNull(domoShare);
        //    Thread.Sleep(5000);
        //    Trace.TraceInformation($"current connection state: {domoShare.HACPublic.Connection.ConnectionState.ToString()}");
        //    Assert.AreEqual(enuOmniLinkConnectionState.OnlineSecure, domoShare.HACPublic.Connection.ConnectionState);
        //}

        [Test]
        public void Test_Connect_For_Controller_EventIsRaised()
        {
            List<string> receivedEvents = new List<string>();
            DomoShare domoShare = new DomoShare();
            domoShare.ControllerConnected += delegate (object sender, ControllerConnectedEventArgs e)
            {
                Assert.Pass();
                return;
            };
            Thread.Sleep(5000);
            Assert.Fail("Did not fire Connected event in time.");
        }

        [Test]
        public void Test_Connect_For_Thermostat_EventIsRaised()
        {
            List<string> receivedEvents = new List<string>();
            DomoShare domoShare = new DomoShare();
            domoShare.ThermostatConnected += delegate (object sender, ThermostatConnectedEventArgs e)
            {
                Assert.AreEqual("5leT4vtD8y5itr8KCJhj0mO4TyPnaRok", e.ThermostatId);
                return;
            };
            Thread.Sleep(15000);
            Assert.Fail("Did not fire Thermostat connected event in time");
        }

        [Test]
        public void Test_Disconnect()
        {
            var moqHAC = new Mock<IHAC>();
            DomoTrollerShare2.DomoShare domoShare = new DomoTrollerShare2.DomoShare();
        }
    }
}
