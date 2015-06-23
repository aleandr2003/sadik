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
using System.Text;
using System.Web.Optimization;

namespace Sadik.Controllers
{
    public class ObservationController : KindergartenDependentController
    {
        public ActionResult Index(int Id)
        {
            using (var context = new SadikEntities())
            {
                if (Request.IsAjaxRequest())
                {
                    var teacherIds = context.UserKindergartens.Where(uk => uk.KindergartenId == KindergartenId)
                        .Select(uk => uk.UserId);
                    var Teachers = context.Users.Where(u => teacherIds.Contains(u.Id))
                        .ToDictionary(u=> u.Id, u => u);

                    var activities = context.fn_ActivitiesWithDuration(Id).ToList();
                    var cameInClass = context.CameInClasses.Include("User").Where(a => a.KidId == Id).ToList();
                    var emotions = context.EmotionObservations.Include("User").Where(e => e.KidId == Id).ToList();
                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    StringBuilder sb = new StringBuilder(32768);
                    sb.Append("[");
                    var timeTemplate = "STRyyyy-MM-dd-HH-mm-ss";
                    for (int i = 0, l = activities.Count; i < l; i++)
                    {
                        var observation = activities[i];
                        var TeacherName = Teachers.ContainsKey(observation.TeacherId ?? -1) ? Teachers[observation.TeacherId ?? -1].FirstName : "";
                        sb.Append(serializer.Serialize(new
                        {
                            Id = observation.Id,
                            KidId = observation.KidId,
                            ItemId = observation.ItemId,
                            Comment = observation.Comment,
                            //используем префикс, чтобы правильно опознать формат и распарсить на клиенте.
                            DateObserved = observation.DateObserved.ToString(timeTemplate),
                            Duration = observation.DurationCalculated,
                            DurationMinutes = observation.DurationMinutes,
                            Polarization = observation.Polarization,
                            ChoseHimSelf = observation.ChoseHimSelf,
                            TeacherId = observation.TeacherId,
                            TeacherName = TeacherName,
                            UniqueId = observation.UniqueId,
                            Type = "Activity"
                        }));
                        sb.Append(",");
                    }
                    for (int i = 0, l = cameInClass.Count; i < l; i++)
                    {
                        var observation = cameInClass[i];
                        sb.Append(serializer.Serialize(new
                        {
                            Id = observation.Id,
                            KidId = observation.KidId,
                            UniqueId = observation.UniqueId,
                            //используем префикс, чтобы правильно опознать формат и распарсить на клиенте.
                            DateObserved = observation.DateTimeCameInClass.ToString(timeTemplate),
                            TeacherId = observation.User.Id,
                            TeacherName = observation.User.FirstName,
                            Comment = observation.Comment,
                            Type = "CameInClass"
                        }));
                        sb.Append(",");
                    }
                    for (int i = 0, l = emotions.Count; i < l; i++)
                    {
                        var observation = emotions[i];
                        sb.Append(serializer.Serialize(new
                        {
                            Id = observation.Id,
                            KidId = observation.KidId,
                            UniqueId = observation.UniqueId,
                            //используем префикс, чтобы правильно опознать формат и распарсить на клиенте.
                            DateObserved = observation.DateObserved.ToString(timeTemplate),
                            Emotion = (int)observation.Emotion,
                            TeacherId = observation.User.Id,
                            TeacherName = observation.User.FirstName,
                            Comment = observation.Comment,
                            Type = "Emotion"
                        }));
                        sb.Append(",");
                    }
                    if (activities.Any() || cameInClass.Any() || emotions.Any()) { sb.Remove(sb.Length - 1, 1); }// если есть хоть один элемент, удаляем последнюю запятую.
                    sb.Append("]");
                    return Content(sb.ToString());
                }
                else
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
        }
        //a temporary method to test observation list with angular
        public ActionResult IndexAngular(int Id)
        {
            using (var context = new SadikEntities())
            {
                //var activities = context.Activities.Include("Inventory").Include("User").Where(a => a.KidId == Id).ToList();
                //var cameInClass = context.CameInClasses.Include("User").Where(a => a.KidId == Id).ToList();
                //var emotions = context.EmotionObservations.Include("User").Where(e => e.KidId == Id).ToList();
                //var observations = new List<ObservationModel>();
                //observations.AddRange(activities.Select(a => new ObservationActivityModel(a)));
                //observations.AddRange(cameInClass.Select(c => new ObservationCameInClassModel(c)));
                //observations.AddRange(emotions.Select(e => new ObservationEmotionModel(e)));
                //var ordered = observations.OrderByDescending(o => o.DateObserved).ToList();
                ViewBag.KidId = Id;
                //return View("IndexAngular", ordered);
                return View("IndexAngular");
            }
        }

        [HttpPost]
        public ActionResult DeleteActivity(Guid UniqueId)
        {
            return DeleteActivityMethod(UniqueId);
        }

        [HttpDelete]
        public ActionResult Activity(Guid UniqueId)
        {
            return DeleteActivityMethod(UniqueId);
        }

        public ActionResult DeleteActivityMethod(Guid UniqueId)
        {
            using (var context = new SadikEntities())
            {
                var activity = context.Activities.Include("Kid").FirstOrDefault(a => a.UniqueId == UniqueId);
                if (activity == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно она уже удалена.", UniqueId);
                    return RedirectToAction("Index", "Error");
                }

                var kidId = activity.KidId;
                if (!authz.Authorize(Operation.ManageObservationNotes, activity.Kid))
                    throw new UnauthorizedAccessException("Удалять записи могут только воспитатели");
                context.Activities.Remove(activity);
                context.SaveChanges();
                if (Request.IsAjaxRequest())
                {
                    return Json(new { message = "Наблюдение удалено", UniqueId = activity.UniqueId });
                }
                else
                {
                    return RedirectToAction("View", "Kids", new { Id = kidId, KindergartenId = KindergartenId });
                }
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
                            DurationMinutes = model.DurationMinutes ?? 0,
                            DateObserved = model.DateObserved,
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
                    activity.DurationMinutes = model.DurationMinutes ?? 0;
                    activity.DateObserved = model.DateObserved;
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
                            DateTimeCameInClass = model.DateObserved,
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
                    observation.DateTimeCameInClass = model.DateObserved;
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
        public ActionResult DeleteCameInClass(Guid UniqueId)
        {
            return DeleteCameInClassMethod(UniqueId);
        }

        [HttpDelete]
        public ActionResult CameInClass(Guid UniqueId)
        {
            return DeleteCameInClassMethod(UniqueId);
        }

        public ActionResult DeleteCameInClassMethod(Guid UniqueId)
        {
            using (var context = new SadikEntities())
            {
                var observation = context.CameInClasses.Include("Kid").FirstOrDefault(o => o.UniqueId == UniqueId);
                if (observation == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно она уже удалена.", UniqueId);
                    return RedirectToAction("Index", "Error");
                }

                var kidId = observation.KidId;
                if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                    throw new UnauthorizedAccessException("Удалять записи могут только воспитатели");
                context.CameInClasses.Remove(observation);
                context.SaveChanges();
                if (Request.IsAjaxRequest())
                {
                    return Json(new { message = "Наблюдение удалено", UniqueId = observation.UniqueId });
                }
                else
                {
                    return RedirectToAction("View", "Kids", new { Id = kidId, KindergartenId = KindergartenId });
                }
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
                            DateObserved = model.DateObserved,
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
                    observation.DateObserved = model.DateObserved;
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
        public ActionResult DeleteEmotion(Guid UniqueId)
        {
            return DeleteEmotionMethod(UniqueId);
        }

        [HttpDelete]
        public ActionResult Emotion(Guid UniqueId)
        {
            return DeleteEmotionMethod(UniqueId);
        }

        public ActionResult DeleteEmotionMethod(Guid UniqueId)
        {
            using (var context = new SadikEntities())
            {
                var observation = context.EmotionObservations.Include("Kid").FirstOrDefault(o => o.UniqueId == UniqueId);
                if (observation == null)
                {
                    TempData["ErrorMessage"] = String.Format("Запись с номером {0} отсутствует. Возможно она уже удалена.", UniqueId);
                    return RedirectToAction("Index", "Error");
                }

                var kidId = observation.KidId;
                if (!authz.Authorize(Operation.ManageObservationNotes, observation.Kid))
                    throw new UnauthorizedAccessException("Удалять записи могут только воспитатели");
                context.EmotionObservations.Remove(observation);
                context.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { message = "Наблюдение удалено", UniqueId = observation.UniqueId });
                }
                else
                {
                    return RedirectToAction("View", "Kids", new { Id = kidId, KindergartenId = KindergartenId });
                }
            }
        }

        [HttpGet]
        public JsonResult Skill(int KidId, int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId && !k.IsDismissed);
                if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");

                var skill = kid.GetSkill(ItemId);
                return Json(new { skill = (int)skill }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Skill(int KidId, int ItemId, int skillDegree)
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

        [HttpGet]
        public JsonResult Presentation(int KidId, int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var kid = context.Kids.FirstOrDefault(k => k.Id == KidId && !k.IsDismissed);
                if (kid == null) throw new ArgumentException("Ребенок не найден. Возможно его профиль был удален");
                var presentation = kid.GetPresentation(ItemId);
                if (presentation != null)
                {
                    return Json(new { presentation = true, date = presentation.DatePerformed.ToString("dd-MM-yyyy") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { presentation = false, date = "" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult Presentation(int KidId, int ItemId, bool presPerformed, DateTime? datePerformed)
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

        [HttpGet]
        public JsonResult ItemUsageDetails(int KidId, int ItemId)
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
                }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult ItemUsageDetails(LogItemUsageDetailsModel model)
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
