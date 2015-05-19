using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models.Notifications
{
    public abstract class NotificationTemplate
    {
        public abstract string Email { get; }
        public abstract string Subject { get; }
        public abstract string Body { get; }
    }
}