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
    public class KidsController : KindergartenDependentController
    {
        public ActionResult Index()
        {
            using (var context = new SadikEntities())
            {
                var kids = context.Kids.Where(k => !k.IsDismissed && k.KindergartenId == KindergartenId)
                    .OrderBy(k => k.LastName).ThenBy(k => k.FirstName).ToList();
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                ViewBag.KindergartenId = KindergartenId;
                ViewBag.CanAcceptKids = authz.Authorize(Operation.AcceptKids, KindergartenId);
                return View("Index", kids);
            }
        }

        public ActionResult IndexShortJsonSerialized()
        {
            using (var context = new SadikEntities())
            {
                var kids = context.Kids.Where(k => !k.IsDismissed && k.KindergartenId == KindergartenId)
                    .Select(k => new
                    {
                        k.FirstName,
                        k.LastName,
                        k.Id
                    }).ToList().Select(k => new 
                    {
                        FullName = String.Format("{0} {1}", k.LastName, k.FirstName),
                        Id = k.Id.ToString()
                    }).OrderBy(k => k.FullName).ToList();
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                string sJSON = serializer.Serialize(kids);// пришлось использовать такой хак, потому что если вставлять вызов на action возвращающий json в шаблон razor, подменяется content-type и браузер воспринимает всю страницу как json
                return Content(sJSON);
            }
        }

        [HttpPost]
        public ActionResult Accept(AcceptKidModel model)
        {
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            if (!authz.Authorize(Operation.AcceptKids, KindergartenId))
                throw new UnauthorizedAccessException("Принимать новых детей могут только воспитатели");
            using (var context = new SadikEntities())
            {
                var kid = new Kid
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Patronymic = model.Patronymic,
                    DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                    DateAccepted = model.DateAccepted ?? DateTime.Now,
                    KindergartenId = KindergartenId
                };

                context.Kids.Add(kid);
                context.SaveChanges();
                return RedirectToAction("Edit", "Kindergarten", new { Id = KindergartenId });
            }
        }

        public ActionResult Dismiss(int Id)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id);
                if (kid == null) RedirectToAction("Index", "Kids", new { KindergartenId = KindergartenId });
                if (!authz.Authorize(Operation.ManageKids, kid))
                    throw new UnauthorizedAccessException("Удалять детей могут только воспитатели");
                kid.IsDismissed = true;
                kid.DateDismissed = DateTime.Now;
                context.SaveChanges();
            }
            return RedirectToAction("Edit", "Kindergarten", new { Id = KindergartenId });
        }

        public ActionResult Restore(int Id)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id);
                if (kid == null) RedirectToAction("Index", "Kids", new { KindergartenId = KindergartenId });
                if (!authz.Authorize(Operation.ManageKids, kid))
                    throw new UnauthorizedAccessException("Работать с детьми могут только воспитатели");
                kid.IsDismissed = false;
                kid.DateDismissed = DateTime.Now;
                context.SaveChanges();
            }
            return RedirectToAction("Edit", "Kindergarten", new { Id = KindergartenId });
        }
        [HttpGet]
        public ActionResult View(int Id)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id);
                if (kid == null)
                {
                    TempData["ErrorMessage"] = String.Format("Ребенок с номером {0} отсутствует. Возможно, его профиль удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                ViewBag.CanAddObservation = authz.Authorize(Operation.AddObsevations, kid);
                ViewBag.CanAddADR = authz.Authorize(Operation.ManageADRs, kid);
                return View("View", kid);
            }
        }

        [HttpGet]
        public ActionResult EditKid(int Id)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id);
                if (kid == null)
                {
                    TempData["ErrorMessage"] = String.Format("Ребенок с номером {0} отсутствует. Возможно, его профиль удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageKids, kid))
                    throw new UnauthorizedAccessException("Редактировать профили детей могут только воспитатели");
                var model = new EditKidModel()
                {
                    Id = kid.Id,
                    FirstName = kid.FirstName,
                    LastName = kid.LastName,
                    Patronymic = kid.Patronymic,
                    DateOfBirth = kid.DateOfBirth,
                    DateAccepted = kid.DateAccepted
                };

                return View("EditKid", model);
            }
        }

        [HttpPost]
        public ActionResult EditKid(int Id, EditKidModel model)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(u => u.Id == Id);
                if (kid == null)
                {
                    TempData["ErrorMessage"] = String.Format("Ребенок с номером {0} отсутствует. Возможно, его профиль удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageKids, kid))
                    throw new UnauthorizedAccessException("Редактировать профили детей могут только воспитатели");
                kid.FirstName = model.FirstName;
                kid.LastName = model.LastName;
                kid.Patronymic = model.Patronymic;
                kid.DateOfBirth = model.DateOfBirth;
                kid.DateAccepted = model.DateAccepted;

                context.SaveChanges();

                return RedirectToAction("View", "Kids", new { Id = kid.Id, KindergartenId=KindergartenId });
            }
        }

        public KidsController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Kids;
        }

        readonly IAuthorizationService authz;
    }
}
