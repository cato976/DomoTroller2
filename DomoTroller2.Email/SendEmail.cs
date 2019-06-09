using System;
using System.Net.Mail;

namespace DomoTroller2.Email
{
    public class SendEmail
    {
        public static bool SendEmailToUser(MailAddress mailAddress, MailMessage mailMessage, string smtpServer, int smptPort, string senderAccount, string password)
        {
            SmtpClient smtpclnt = new SmtpClient(smtpServer, smptPort);
            smtpclnt.EnableSsl = true;
            System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(senderAccount, password);
            smtpclnt.Credentials = SMTPUserInfo;

            smtpclnt.Send(mailMessage);

            return true;
        }
    }
}
