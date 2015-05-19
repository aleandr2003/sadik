using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Application.Validation
{
    public static class SharedRegexPatterns
    {
        public const string EmailPattern = @"^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\.(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$";
    }
}