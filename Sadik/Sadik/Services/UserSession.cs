using Sadik.Models;
using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Sadik.Services
{
    public class UserSession : IUserSession
    {
        public UserSession()
        {
            var login = HttpContext.Current.User.Identity.Name;
            using (var context = new SadikEntities())
            {
                CurrentUser = context.Users.Where(u => u.Email == login).FirstOrDefault();
                KindergartenIds = new List<int>();
                if (CurrentUser != null)
                {
                    KindergartenIds.AddRange(CurrentUser.UserKindergartens.Select(uk => uk.KindergartenId));
                }
            }
            IsAuthenticated = CurrentUser != null;
        }

        public void LogIn(User user)
        {
            CurrentUser = user;
            FormsAuthentication.SetAuthCookie(user.Email, true);
        }

        public void LogOut()
        {
            FormsAuthentication.SignOut();
        }

        public User CurrentUser
        {
            get;
            private set;
        }

        public List<int> KindergartenIds
        {
            get;
            private set;
        }

        public int? UserId
        {
            get
            {
                return IsAuthenticated ? (int?)CurrentUser.Id : null;
            }
        }

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public User Authenticate(string login, string password)
        {
            User user;
            using (var context = new SadikEntities())
            {
                user = context.Users.Where(u => u.Email == login).FirstOrDefault();
            }
            if (user == null)
            {
                throw new ArgumentException("Неправильная пара логин-пароль");
            }
            else
            {
                if (!user.PasswordMatches(password))
                {
                    throw new ArgumentException("Неправильная пара логин-пароль");
                }
            }
            
            return user;
        }

    }
}