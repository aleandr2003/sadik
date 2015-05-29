using Sadik.Models;
using Sadik.Services;
using Sadik.Services.Abstract;
using Sadik.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sadik.Extensions;
using Sadik.ViewModels.Observations;

namespace Sadik.Controllers
{
    public class ObservationController : KindergartenDependentController
    {
        public ActionResult Index(int Id)
        {
            using (var context = new SadikEntities())
            {
                var activities = context.Activities.Include("Inventory").Include("User").Where(a => a.KidId == Id).ToList();
                var cameInClass = context.CameInClasses.Include("User").Where(a => a.KidId == Id).ToList();
                var emotions = context.EmotionObservations.Include("User").Where(e => e.KidId == Id).ToList();
                var observations = new List<ObservationModel>();
                observations.AddRange(activities.Select(a => new ObservationActivityModel(a)));
                observations.AddRange(cameInClass.Select(c => new ObservationCameInClassModel(c)));
                observations.AddRange(emotions.Select(e => new ObservationEmotionModel(e)));

                return View("Index", observations.OrderByDescending(o => o.DateObserved));
            }
        }

        [HttpPost]
        public ActionResult DeleteActivity(int Id)
        {
            using (var context = new SadikEntities())
            {
                var activity = context.Activities.Include("Kid").FirstOrDefault(a => a.Id == Id);
                if (activity == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно она уже удалена.", Id);
                    return RedirectToAction("Index", "Error");
                }

                var kidId = activity.KidId;
                if (!authz.Authorize(Operation.ManageObservationNotes, activity.Kid))
                    throw new UnauthorizedAccessException("Удалять записи могут только воспитатели");
                context.Activities.Remove(activity);
                context.SaveChanges();
                return RedirectToAction("View","Kids", new {Id = kidId, KindergartenId = KindergartenId });
            }
        }

        [HttpGet]
        public ActionResult Log()
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!authz.Authorize(Operation.AddObsevations, KindergartenId))
                throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");
            ViewBag.KindergartenId = KindergartenId;
            return View();
        }

        [HttpGet]
        public ActionResult LogActivity(string returnUrl)
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!authz.Authorize(Operation.AddObsevations, KindergartenId))
                throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Activity(LogActivityModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.KidId == null) throw new ArgumentException("Ребенок не выбран");
                if (model.ItemId == null) throw new ArgumentException("Материал не выбран");
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
                DateTime DateObserved = model.GetObservationDate();

                using (var context = new SadikEntities())
                {
                    var kid = context.Kids.FirstOrDefault(k => k.Id == model.KidId && !k.IsDismissed);
                    if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                    if (!authz.Authorize(Operation.AddObsevations, kid))
                        throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");

                    if (!context.Activities.Any(o => o.UniqueId == model.UniqueId && o.KidId == model.KidId))
                    {
                        var activity = new Activity
                        {
                            KidId = model.KidId ?? 0,
                            ItemId = model.ItemId,
                            Duration = model.Duration,
                            DateObserved = DateObserved,
                            Comment = model.Comment,
                            TeacherId = userSession.CurrentUser.Id,
                            Polarization = model.Polarization,
                            ChoseHimSelf = model.ChoseHimSelf,
                            UniqueId = model.UniqueId
                        };

                        context.Activities.Add(activity);
                        context.SaveChanges();
                    }
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { message = "Наблюдение сохранено", UniqueId = model.UniqueId });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Log", "Observation", new { KindergartenId = KindergartenId });
                    }
                }
            }
            catch(Exception ex){
                if ((ex is ArgumentException || ex is UnauthorizedAccessException) && Request.IsAjaxRequest())
                {
                    return Json(new { message = ex.Message });
                }
                else
                {
                    throw ex;
                }
            }
            
        }

        [HttpGet]
        public ActionResult EditActivity(int id, string returnUrl)
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            
            using (var context = new SadikEntities())
            {
                var activity = context.Activities.Include("Kid").Include("Inventory").FirstOrDefault(a => a.Id == id);
                if (activity == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно её уже удалили.", id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageObservationNotes, activity.Kid))
                    throw new UnauthorizedAccessException("Редактировать записи могут только воспитатели");

                ViewBag.ReturnUrl = returnUrl;
                return View(activity);
            }
        }

        [HttpPost]
        public ActionResult EditActivity(int id, LogActivityModel model, string returnUrl)
        {
            return EditActivityMethod(id, model, returnUrl);
        }

        [HttpPut]
        public ActionResult Activity(int id, LogActivityModel model, string returnUrl)
        {
            return EditActivityMethod(id, model, returnUrl);
        }

        private ActionResult EditActivityMethod(int id, LogActivityModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.KidId == null) throw new ArgumentException("Ребенок не выбран");
                if (model.ItemId == null) throw new ArgumentException("Материал не выбран");
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
                
                DateTime DateObserved = DateTime.Now;
                if (model.DateObserved.HasValue)
                {
                    DateObserved = model.DateObserved.Value;
                    if (model.Hours.HasValue) DateObserved = DateObserved.AddHours(model.Hours.Value);
                    if (model.Minutes.HasValue) DateObserved = DateObserved.AddMinutes(model.Minutes.Value);
                }
                using (var context = new SadikEntities())
                {
                    var activity = context.Activities.FirstOrDefault(a => a.Id == id);
                    if (activity == null)
                    {
                        TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно её уже удалили.", id);
                        return RedirectToAction("Index", "Error");
                    }
                    if (!authz.Authorize(Operation.ManageObservationNotes, activity.Kid))
                        throw new UnauthorizedAccessException("Редактировать записи могут только воспитатели");

                    activity.KidId = model.KidId ?? 0;
                    activity.ItemId = model.ItemId;
                    activity.Duration = model.Duration;
                    activity.DateObserved = DateObserved;
                    activity.Comment = model.Comment;
                    activity.TeacherId = userSession.CurrentUser.Id;
                    activity.Polarization = model.Polarization;
                    activity.ChoseHimSelf = model.ChoseHimSelf;

                    context.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { message = "Изменения сохранены", returnUrl = returnUrl, UniqueId = model.UniqueId });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("View", "Kids", new { Id = model.KidId, KindergartenId= KindergartenId });
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex is ArgumentException || ex is UnauthorizedAccessException) && Request.IsAjaxRequest())
                {
                    return Json(new { message = ex.Message });
                }
                else
                {
                    throw ex;
                }
            }
        }

        [HttpGet]
        public ActionResult LogCameInClass(string returnUrl)
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!authz.Authorize(Operation.AddObsevations, KindergartenId))
                throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult CameInClass(LogCameInClassModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.KidId == null) throw new ArgumentException("Ребенок не выбран");
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");

                DateTime DateObserved = model.GetObservationDate();

                using (var context = new SadikEntities())
                {
                    var kid = context.Kids.FirstOrDefault(k => k.Id == model.KidId && !k.IsDismissed);
                    if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                    if (!authz.Authorize(Operation.AddObsevations, kid))
                        throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");

                    if (!context.CameInClasses.Any(o => o.UniqueId == model.UniqueId && o.KidId == model.KidId))
                    {
                        var observation = new CameInClass
                        {
                            KidId = model.KidId ?? 0,
                            DateTimeCameInClass = DateObserved,
                            Comment = model.Comment,
                            TeacherId = userSession.CurrentUser.Id,
                            UniqueId = model.UniqueId
                        };

                        context.CameInClasses.Add(observation);
                        context.SaveChanges();
                    }
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { message = "Наблюдение сохранено", UniqueId = model.UniqueId });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Log", "Observation", new { KindergartenId = KindergartenId });
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex is ArgumentException || ex is UnauthorizedAccessException) && Request.IsAjaxRequest())
                {
                    return Json(new { message = ex.Message });
                }
                else
                {
                    throw ex;
                }
            }
        }

        [HttpGet]
        public ActionResult EditCameInClass(int id, string returnUrl)
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var observation = context.CameInClasses.Include("Kid").FirstOrDefault(o => o.Id == id);
                if (observation == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно её уже удалили.", id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                    throw new UnauthorizedAccessException("Редактировать записи могут только воспитатели");
                ViewBag.ReturnUrl = returnUrl;
                return View(observation);
            }
        }

        [HttpPost]
        public ActionResult EditCameInClass(int id, LogCameInClassModel model, string returnUrl)
        {
            return EditCameInClassMethod(id, model, returnUrl);
        }
        [HttpPut]
        public ActionResult CameInClass(int id, LogCameInClassModel model, string returnUrl)
        {
            return EditCameInClassMethod(id, model, returnUrl);
        }

        private ActionResult EditCameInClassMethod(int id, LogCameInClassModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.KidId == null) throw new ArgumentException("Ребенок не выбран");
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
                
                DateTime DateObserved = model.GetObservationDate();
                using (var context = new SadikEntities())
                {
                    var observation = context.CameInClasses.FirstOrDefault(o => o.Id == id);
                    if (observation == null)
                    {
                        TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно её уже удалили.", id);
                        return RedirectToAction("Index", "Error");
                    }
                    if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                        throw new UnauthorizedAccessException("Редактировать записи могут только воспитатели");
                    observation.KidId = model.KidId ?? 0;
                    observation.DateTimeCameInClass = DateObserved;
                    observation.Comment = model.Comment;
                    observation.TeacherId = userSession.CurrentUser.Id;

                    context.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { message = "Изменения сохранены", returnUrl = returnUrl, UniqueId = model.UniqueId });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("View", "Kids", new { Id = model.KidId, KindergartenId = KindergartenId });
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex is ArgumentException || ex is UnauthorizedAccessException) && Request.IsAjaxRequest())
                {
                    return Json(new { message = ex.Message });
                }
                else
                {
                    throw ex;
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteCameInClass(int Id)
        {
            using (var context = new SadikEntities())
            {
                var observation = context.CameInClasses.Include("Kid").FirstOrDefault(o => o.Id == Id);
                if (observation == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно она уже удалена.", Id);
                    return RedirectToAction("Index", "Error");
                }

                var kidId = observation.KidId;
                if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                    throw new UnauthorizedAccessException("Удалять записи могут только воспитатели");
                context.CameInClasses.Remove(observation);
                context.SaveChanges();
                return RedirectToAction("View", "Kids", new { Id = kidId, KindergartenId = KindergartenId });
            }
        }

        [HttpGet]
        public ActionResult LogEmotion(string returnUrl)
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!authz.Authorize(Operation.AddObsevations, KindergartenId))
                throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Emotion(LogEmotionModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.KidId == null) throw new ArgumentException("Ребенок не выбран");
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");

                DateTime DateObserved = model.GetObservationDate();

                using (var context = new SadikEntities())
                {
                    var kid = context.Kids.FirstOrDefault(k => k.Id == model.KidId && !k.IsDismissed);
                    if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                    if (!authz.Authorize(Operation.AddObsevations, kid))
                        throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");
                    if (!context.EmotionObservations.Any(o => o.UniqueId == model.UniqueId && o.KidId == model.KidId))
                    {
                        var observation = new EmotionObservation
                        {
                            KidId = model.KidId ?? 0,
                            DateObserved = DateObserved,
                            Comment = model.Comment,
                            TeacherId = userSession.CurrentUser.Id,
                            Emotion = model.Emotion,
                            UniqueId = model.UniqueId
                        };

                        context.EmotionObservations.Add(observation);
                        context.SaveChanges();
                    }
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { message = "Наблюдение сохранено", UniqueId = model.UniqueId });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Log", "Observation", new { KindergartenId = KindergartenId });
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex is ArgumentException || ex is UnauthorizedAccessException) && Request.IsAjaxRequest())
                {
                    return Json(new { message = ex.Message });
                }
                else
                {
                    throw ex;
                }
            }
        }

        [HttpGet]
        public ActionResult EditEmotion(int id, string returnUrl)
        {
            ViewData["SelectedTab"] = HeaderTabs.Observation;
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            
            using (var context = new SadikEntities())
            {
                var observation = context.EmotionObservations.Include("Kid").FirstOrDefault(o => o.Id == id);
                if (observation == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно её уже удалили.", id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                    throw new UnauthorizedAccessException("Редактировать записи могут только воспитатели");
                ViewBag.ReturnUrl = returnUrl;
                return View(observation);
            }
        }

        [HttpPost]
        public ActionResult EditEmotion(int id, LogEmotionModel model, string returnUrl)
        {
            return EditEmotionMethod(id, model, returnUrl);
        }

        [HttpPut]
        public ActionResult Emotion(int id, LogEmotionModel model, string returnUrl)
        {
            return EditEmotionMethod(id, model, returnUrl);
        }

        private ActionResult EditEmotionMethod(int id, LogEmotionModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.KidId == null) throw new ArgumentException("Ребенок не выбран");
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
                
                DateTime DateObserved = model.GetObservationDate();
                using (var context = new SadikEntities())
                {
                    var observation = context.EmotionObservations.FirstOrDefault(o => o.Id == id);
                    if (observation == null)
                    {
                        TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно её уже удалили.", id);
                        return RedirectToAction("Index", "Error");
                    }
                    if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                        throw new UnauthorizedAccessException("Редактировать записи могут только воспитатели");
                    observation.KidId = model.KidId ?? 0;
                    observation.DateObserved = DateObserved;
                    observation.Comment = model.Comment;
                    observation.TeacherId = userSession.CurrentUser.Id;
                    observation.Emotion = model.Emotion;

                    context.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { message = "Изменения сохранены", returnUrl = returnUrl, UniqueId = model.UniqueId });
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("View", "Kids", new { Id = model.KidId, KindergartenId = KindergartenId });
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex is ArgumentException || ex is UnauthorizedAccessException) && Request.IsAjaxRequest())
                {
                    return Json(new { message = ex.Message });
                }
                else
                {
                    throw ex;
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteEmotion(int Id)
        {
            using (var context = new SadikEntities())
            {
                var observation = context.EmotionObservations.Include("Kid").FirstOrDefault(o => o.Id == Id);
                if (observation == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно она уже удалена.", Id);
                    return RedirectToAction("Index", "Error");
                }

                var kidId = observation.KidId;
                if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                    throw new UnauthorizedAccessException("Удалять записи могут только воспитатели");
                context.EmotionObservations.Remove(observation);
                context.SaveChanges();
                return RedirectToAction("View", "Kids", new { Id = kidId, KindergartenId = KindergartenId });
            }
        }

        public JsonResult GetSkill(int KidId, int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId && !k.IsDismissed);
                if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");

                var skill = kid.GetSkill(ItemId);
                return Json(new { skill = (int)skill });
            }
        }

        [HttpPost]
        public JsonResult UpdateSkill(int KidId, int ItemId, int skillDegree)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var kid = context.Kids.FirstOrDefault(k => k.Id == KidId && !k.IsDismissed);
                    if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                    if (!authz.Authorize(Operation.AddObsevations, kid))
                        throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");
                    kid.UpdateSkill(ItemId, skillDegree);
                    context.SaveChanges();
                }
                return Json(new { result = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { result = false });
            }
        }

        public JsonResult GetPresentation(int KidId, int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId && !k.IsDismissed);
                if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                var presentation = kid.GetPresentation(ItemId);
                if (presentation != null)
                {
                    return Json(new { presentation = true, date = presentation.DatePerformed.ToString("dd-MM-yyyy") });
                }
                else
                {
                    return Json(new { presentation = false, date = "" });
                }
            }
        }

        [HttpPost]
        public JsonResult UpdatePresentation(int KidId, int ItemId, bool presPerformed, DateTime? datePerformed)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var kid = context.Kids.FirstOrDefault(k => k.Id == KidId && !k.IsDismissed);
                    if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                    if (!authz.Authorize(Operation.AddObsevations, kid))
                        throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");
                    kid.UpdatePresentation(ItemId, presPerformed, datePerformed);
                    context.SaveChanges();
                }
                return Json(new { result = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { result = false });
            }
        }

        public JsonResult GetItemUsageDetails(int KidId, int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var result = Kid.GetItemUsageDetails(KidId, ItemId);
                var skill = result.Item1;
                var presentation = result.Item2;

                return Json(new { 
                    skill = skill, 
                    presentation = presentation != null,
                    date = presentation != null ? presentation.DatePerformed.ToString("dd-MM-yyyy") : "",
                    KidId = KidId,
                    ItemId = ItemId
                });

            }
        }

        [HttpPost]
        public JsonResult UpdateItemUsageDetails(LogItemUsageDetailsModel model)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var kid = context.Kids.FirstOrDefault(k => k.Id == model.KidId && !k.IsDismissed);
                    if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                    if (!authz.Authorize(Operation.AddObsevations, kid))
                        throw new UnauthorizedAccessException("Добавлять записи могут только воспитатели");
                    if (!context.ItemUsageDetails.Any(d => d.UniqueId == model.UniqueId))
                    {
                        kid.UpdateSkill(model.ItemId, model.skillDegree);
                        kid.UpdatePresentation(model.ItemId, model.presPerformed, model.datePerformed);
                        var details = new ItemUsageDetail() { 
                            UniqueId = model.UniqueId,
                            KidId = model.KidId,
                            ItemId = model.ItemId,
                            PresentationDate = model.presPerformed ? model.datePerformed : null,
                            Degree = model.skillDegree
                        };
                        context.ItemUsageDetails.Add(details);
                        context.SaveChanges();
                    }
                }
                return Json(new { result = true, UniqueId = model.UniqueId });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { result = false, UniqueId = model.UniqueId });
            }
        }

        public ActionResult Stats(int Id, DateTime? startPeriod, DateTime? endPeriod)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id);
                if (kid == null) throw new ArgumentException(String.Format("Ребенок с номером {0} отсутствует", Id));
                ViewBag.Kid = kid;
                var report = new StatReport(kid, startPeriod, endPeriod);

                ViewBag.ZoneTreeDepth = report.GetZoneDepth;

                return View("StatsTable", report.ComputeZoneStats());
            }
        }

        public ActionResult ActivityCharts(int Id)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == Id);
                if (kid == null)
                    throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                var activities = context.Activities.Include("Inventory").Where(a => a.KidId == Id).ToList();
                var rootZone = context.Zones.FirstOrDefault(z => z.ParentZoneId == null && z.KindergartenId == kid.KindergartenId);
                if (rootZone == null)
                    throw new ArgumentException("Ошибка данных. Корневая зона не найдена");
                var mainZones = rootZone.ChildZones.ToList();

                var charts = new Dictionary<string, ChartDataItem[]>();
                foreach (var zone in mainZones)
                {
                    var activitiesFiltered = activities.Where(a => a.Inventory.ZonePath.Select(z => z.Id).Contains(zone.Id)).ToList();
                    int total = 0;
                    var chartData = activitiesFiltered.OrderBy(a => a.DateObserved).Select(a => a.DateObserved.ToString("yyyy-MM-dd"))
                        .GroupBy(a => a)
                        .Select(g => new { date = g.Key, count = g.Count() })
                        .Select(g => new ChartDataItem() { date = g.date, count = total += g.count }).ToArray();
                    charts[zone.Name] = chartData;
                }
                return Json(charts.Select(kvp => new {zone = kvp.Key, chart = kvp.Value }).ToArray());
            }
        }

        public ActionResult CameInClassChart(int Id)
        {
            using (var context = new SadikEntities())
            {
                var observations = context.CameInClasses.Where(o => o.KidId == Id)
                    .Select(o => o.DateTimeCameInClass).ToArray().OrderBy(date => date).Select(date => date.Ticks.ToString()).ToArray();
                return Json(observations);
            }
        }

        

        public ObservationController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
            
        }

        readonly IAuthorizationService authz;

        public struct ChartDataItem
        {
            public string date;
            public int count;
        }

    }
}
