using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System;
using System.IO;

using Should;
using System.Threading;

namespace Tests
{
    public class Tests
    {
        private static IConfigurationRoot Configuration;

        [SetUp]
        public void Setup()
        {
            string path = GetPath();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(path))
                .AddJsonFile("appsettings.json", false, true);
            Configuration = builder.Build();
        }

        [Test]
        public void Test_Connect()
        {
            DomoTrollerShare2.DomoShare domoShare =  new DomoTrollerShare2.DomoShare();
            Assert.IsNotNull(domoShare);
            Thread.Sleep(15000);
            var connected = domoShare.Connected;
            domoShare.Connected.ShouldBeTrue();
        }

        public static string GetPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return path;
        }
    }
}
