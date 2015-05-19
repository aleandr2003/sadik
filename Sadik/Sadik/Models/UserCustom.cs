using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public partial class User
    {
        private Password _password;
        private Password Password
        {
            get
            {
                if (_password == null)
                {
                    _password = new Password(PasswordHash, PasswordSalt);
                }
                return _password;
            }
        }

        public void SetPassword(string newPassword, string oldPassword)
        {
            if (PasswordHash != null && PasswordSalt != null && !Password.Matches(oldPassword))
                throw new ArgumentException("old password doesn't match");
            var pass = new Password(newPassword);
            PasswordHash = pass.Hash;
            PasswordSalt = pass.Salt;
        }

        public void SetPassword(string newPassword)
        {
            var pass = new Password(newPassword);
            PasswordHash = pass.Hash;
            PasswordSalt = pass.Salt;
        }

        public bool PasswordMatches(string input)
        {
            return Password.Matches(input);
        }
    }
}