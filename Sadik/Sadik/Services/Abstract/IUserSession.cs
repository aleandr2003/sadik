using Sadik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Services.Abstract
{
    public interface IUserSession
    {
        void LogIn(User user);
        void LogOut();
        User CurrentUser { get; }
        List<int> KindergartenIds { get; }
        int? UserId { get; }
        bool IsAuthenticated { get; }
        User Authenticate(string login, string password);
    }
}