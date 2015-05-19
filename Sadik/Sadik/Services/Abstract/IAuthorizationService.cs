using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Services.Abstract
{
    public interface IAuthorizationService
    {
        bool Authorize(Operation op, object obj);

        bool Authorize(Operation op);

        bool Authorize(Operation op, int KindergartenId);
    }
}