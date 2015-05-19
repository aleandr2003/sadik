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
    public class ADRController : KindergartenDependentController
    {
        public ActionResult Index(int Id)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id && !k.IsDismissed);
                if (kid == null)
                {
                    TempData["ErrorMessage"] = String.Format("Ребенок с номером {0} отсутствует. Возможно, его профиль удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                var adrRecords = context.ADRs.Where(adr => adr.KidId == Id).OrderByDescending(adr => adr.Date).ToList();
                ViewBag.Kid = kid;
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View("Index", adrRecords);
            }
        }

        public ActionResult UpcomingIndex()
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var nextWeek = DateTime.Now.AddDays(30);
                var adrRecords = (from kids in context.Kids
                                  join adrs in context.ADRs on kids.Id equals adrs.KidId
                                 where kids.KindergartenId == KindergartenId
                                 && adrs.Date < nextWeek && !adrs.Performed
                                 orderby adrs.Date descending
                                 select adrs).ToList();
                foreach (var adr in adrRecords)
                {
                    var kid = adr.Kid; // simply to upload related kids;
                }
                return View("UpcomingIndex", adrRecords);
            }
        }

        [HttpGet]
        public ActionResult Add(int KidId)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId);
                if (kid == null)
                {
                    TempData["ErrorMessage"] = String.Format("Ребенок с номером {0} отсутствует. Возможно, его профиль удалили.", KidId);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageADRs, kid))
                    throw new UnauthorizedAccessException("Добавлять ДТР могут только воспитатели");
                return View(new ADRModel() { KidId = KidId });
            }
        }

        [HttpPost]
        public ActionResult AddPost(ADRModel model)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == model.KidId);
                if (kid == null)
                {
                    TempData["ErrorMessage"] = String.Format("Ребенок с номером {0} отсутствует. Возможно, его профиль удалили.", model.KidId);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageADRs, kid))
                    throw new UnauthorizedAccessException("Добавлять ДТР могут только воспитатели");
                var ADRRecord = new ADR
                {
                    KidId = model.KidId,
                    Date = model.Date ?? DateTime.Now,
                    AchievementsTeachers = model.AchievementsTeachers,
                    AchievementsParents = model.AchievementsParents,
                    DifficultiesTeachers = model.DifficultiesTeachers,
                    DifficultiesParents = model.DifficultiesParents,
                    RecommendationsTeachers = model.RecommendationsTeachers,
                    RecommendationsParents = model.RecommendationsParents,
                    Performed = model.Performed,
                    StartPeriod = model.StartPeriod,
                    EndPeriod = model.EndPeriod
                };

                context.ADRs.Add(ADRRecord);
                context.SaveChanges();
                return RedirectToAction("View", "Kids", new {id = model.KidId, KindergartenId = KindergartenId });
            }
        }

        [HttpPost]
        public ActionResult Delete(int Id, int KidId)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var ADRRecord = context.ADRs.FirstOrDefault(adr => adr.Id == Id);
                if (ADRRecord == null) RedirectToAction("View", "Kids", new { id = KidId, KindergartenId = KindergartenId });
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId);
                if (!authz.Authorize(Operation.ManageADRs, kid))
                    throw new UnauthorizedAccessException("Удалять ДТР могут только воспитатели");
                context.ADRs.Remove(ADRRecord);
                context.SaveChanges();
            }
            return RedirectToAction("View", "Kids", new { id = KidId, KindergartenId = KindergartenId });
        }

        [HttpGet]
        public ActionResult Edit(int Id, int KidId)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            using (var context = new SadikEntities())
            {
                var ADRRecord = context.ADRs.Include("Kid").FirstOrDefault(adr => adr.Id == Id);
                if (ADRRecord == null) RedirectToAction("View", "Kids", new { id = KidId , KindergartenId = KindergartenId});
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId);
                if (!authz.Authorize(Operation.ManageADRs, kid))
                    throw new UnauthorizedAccessException("Редактировать ДТР могут только воспитатели");
                
                ViewBag.KidsName = ADRRecord.Kid.FirstName + " " + ADRRecord.Kid.LastName;
                var model = new ADRModel()
                {
                    Id = Id,
                    KidId = KidId,
                    Date = ADRRecord.Date,
                    AchievementsTeachers = ADRRecord.AchievementsTeachers,
                    AchievementsParents = ADRRecord.AchievementsParents,
                    DifficultiesTeachers = ADRRecord.DifficultiesTeachers,
                    DifficultiesParents = ADRRecord.DifficultiesParents,
                    RecommendationsTeachers = ADRRecord.RecommendationsTeachers,
                    RecommendationsParents = ADRRecord.RecommendationsParents,
                    Performed = ADRRecord.Performed,
                    StartPeriod = ADRRecord.StartPeriod,
                    EndPeriod = ADRRecord.EndPeriod
                };
                return View("Edit", model);
            }
        }

        [HttpPost]
        public ActionResult Edit(ADRModel model)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                var ADRRecord = context.ADRs.FirstOrDefault(adr => adr.Id == model.Id);
                if (ADRRecord == null) RedirectToAction("View", "Kids", new { id = model.KidId, KindergartenId = KindergartenId });
                var kid = context.Kids.FirstOrDefault(k => k.Id == model.KidId);
                if (!authz.Authorize(Operation.ManageADRs, kid))
                    throw new UnauthorizedAccessException("Редактировать ДТР могут только воспитатели");

                ADRRecord.Date = model.Date ?? DateTime.Now;
                ADRRecord.AchievementsTeachers = model.AchievementsTeachers;
                ADRRecord.AchievementsParents = model.AchievementsParents;
                ADRRecord.DifficultiesTeachers = model.DifficultiesTeachers;
                ADRRecord.DifficultiesParents = model.DifficultiesParents;
                ADRRecord.RecommendationsTeachers = model.RecommendationsTeachers;
                ADRRecord.RecommendationsParents = model.RecommendationsParents;
                ADRRecord.Performed = model.Performed;
                ADRRecord.StartPeriod = model.StartPeriod;
                ADRRecord.EndPeriod = model.EndPeriod;

                context.SaveChanges();

                return RedirectToAction("View", "Kids", new { Id = kid.Id, KindergartenId = KindergartenId });
            }
        }

        public ADRController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Kids;
        }

        readonly IAuthorizationService authz;

    }
}
