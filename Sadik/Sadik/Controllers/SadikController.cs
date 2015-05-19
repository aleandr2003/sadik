using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public abstract class SadikController : Controller
    {
        protected readonly IUserSession userSession;
        public SadikController(IUserSession userSession)
        {
            this.userSession = userSession;
            ViewBag.currentUser = userSession.CurrentUser;
            ViewBag.userKindergartenIds = userSession.KindergartenIds;
        }
    }
}
