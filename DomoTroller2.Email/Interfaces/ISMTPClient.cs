using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace DomoTroller2.Email.Interfaces
{
    public interface ISMTPClient
    {
        void Send(MailMessage mailMessage);
    }
}
