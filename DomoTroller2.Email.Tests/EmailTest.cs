using NUnit.Framework;
using Microsoft.Extensions.Configuration;

using DomoTroller2.Email;
using Should;
using System;
using System.Reflection;
using System.IO;
using System.Net.Mail;

namespace DomoTroller2.Email.Tests
{
    public class EmailTest
    {
        private IConfigurationRoot Configuration { get; set; }

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
        public void SendEmail_Should_Send_Email()
        {
            var smtpServer = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SMTPServer").Value;
            var smtpPort = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SMTPPort").Value;
            var senderEmail = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SenderEmail").Value;
            var senderPassword = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SenderPassword").Value;

            MailAddress ma = new MailAddress(senderEmail, "Test");
            MailMessage mm = new MailMessage(ma, ma);

            SendEmail.SendEmailToUser(ma, mm, smtpServer, int.Parse(smtpPort), senderEmail, senderPassword).ShouldBeTrue();
        }

        private static string GetPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return path;
        }
    }
}
