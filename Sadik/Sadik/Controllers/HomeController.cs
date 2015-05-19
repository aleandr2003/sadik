using Sadik.Models;
using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class HomeController : SadikController
    {

        public ActionResult Index()
        {
            if (userSession.IsAuthenticated)
            {
                if (userSession.CurrentUser.RoleId == UserRole.Teacher)
                {
                    if (ViewBag.KindergartenId != null)
                    {
                        return RedirectToAction("Log", "Observation", new { KindergartenId = ViewBag.KindergartenId });
                    }
                    else if (userSession.KindergartenIds != null && userSession.KindergartenIds.Count > 0)
                    {
                        return RedirectToAction("Log", "Observation", new { KindergartenId = userSession.KindergartenIds.First() });
                    }
                    else
                    {
                        return RedirectToAction("View", "User", new { Id = userSession.UserId});
                    }
                }
                else if (userSession.CurrentUser.RoleId == UserRole.Admin)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    if (ViewBag.KindergartenId != null)
                    {
                        return RedirectToAction("Index", "Kids", new { KindergartenId = ViewBag.KindergartenId });
                    }
                    else if (userSession.KindergartenIds != null && userSession.KindergartenIds.Count > 0)
                    {
                        return RedirectToAction("Index", "Kids", new { KindergartenId = userSession.KindergartenIds.First() });
                    }
                    else
                    {
                        return RedirectToAction("View", "User", new { Id = userSession.UserId });
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public HomeController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
        }

        readonly IAuthorizationService authz;
    }
}
