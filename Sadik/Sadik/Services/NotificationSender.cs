using Sadik.Models.Notifications;
using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Sadik.Services
{
    public class NotificationSender : INotificationSender
    {
        readonly string fromAddress;
        readonly SmtpClient client;

        public NotificationSender(string fromAddress)
        {
            this.fromAddress = fromAddress;
            client = new SmtpClient();
        }

        public void Send(NotificationTemplate template)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromAddress);
            message.To.Add(new MailAddress(template.Email));
            message.Subject = template.Subject;
            message.Body = template.Body;
            message.IsBodyHtml = true;

            client.Send(message);
        }
    }
}