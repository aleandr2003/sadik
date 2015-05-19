using Sadik.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Services.Abstract
{
    public interface INotificationSender
    {
        void Send(NotificationTemplate template);
    }
}