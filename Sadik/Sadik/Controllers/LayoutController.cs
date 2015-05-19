using Sadik.Models;
using Sadik.Services;
using Sadik.Services.Abstract;
using Sadik.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class LayoutController : SadikController
    {
        public ActionResult Header(int? tab, int? KindergartenId)
        {
            ViewBag.SelectedTab = (HeaderTabs)(tab ?? (int)HeaderTabs.None);
            ViewBag.KindergartenId = KindergartenId;
            ViewBag.CanCreateKindergarten = authz.Authorize(Operation.CreateKindergartens);
            return View("LayoutHeader");
        }

        public ActionResult KindergartenList(int? Id)
        {
            ViewBag.SelectedKindergartenId = Id;
            if (userSession.IsAuthenticated)
            {
                using (var context = new SadikEntities())
                {
                    var kindergartens = (from k in context.Kindergartens
                                         join uk in context.UserKindergartens
                                             on k.Id equals uk.KindergartenId
                                         where uk.UserId == userSession.CurrentUser.Id
                                         select k).ToList();
                    return View("KindergartenList", kindergartens);
                }
            }
            else
            {
                return View("KindergartenList", new List<Kindergarten>());
            }
        }

        public ActionResult KindergartenMenu(int Id, int? tab)
        {
            ViewBag.SelectedTab = (HeaderTabs)(tab ?? (int)HeaderTabs.None);
            return View("KindergartenMenu", Id);
        }

        public LayoutController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
        }
        readonly IAuthorizationService authz;
    }
}
