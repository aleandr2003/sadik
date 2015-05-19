using Sadik.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models.Notifications
{
    public class PasswordResetNotificationTemplate : NotificationTemplate
    {
        private string email;
        private string userName;
        private string password;
        private string bodyTemplate = @"Здравствуйте, {0}. <br/> 
Ваш новый пароль на сайте {1}.<br/> 
С ним вы можете зайти на сайт <a href='{2}'>{3}</a>.

Сменить пароль вы можете на странице 'Мой Профиль'.

С уважением, Садик-Садик :)";

        public PasswordResetNotificationTemplate(string email, string userName, string newPassword)
        {
            this.email = email;
            this.userName = userName;
            this.password = newPassword;
        }

        public override string Email
        {
            get
            {
                return email;
            }
        }

        public override string Subject
        {
            get
            {
                return "Ваш пароль переустановлен.";
            }
        }

        public override string Body
        {
            get
            {
                var urlHelper = UrlHelperExtensions.GetUrlHelper();
                return String.Format(bodyTemplate, userName, password, urlHelper.ActionAbsolute("Index", "Login"), "садик-садик");
            }
        }
    }
}