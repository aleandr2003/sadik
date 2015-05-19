using Sadik.Application.Validation;
using Sadik.Models;
using Sadik.Services;
using Sadik.Services.Abstract;
using Sadik.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class KindergartenController : SadikController
    {
        public ActionResult Index()
        {
            using (var context = new SadikEntities())
            {
                var kindergartens = context.Kindergartens.Where(k => !k.IsDeleted).ToList();
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View("Index", kindergartens);
            }
        }

        public ActionResult View(int Id)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            using (var context = new SadikEntities())
            {
                var kindergarten = context.Kindergartens.FirstOrDefault(k => k.Id == Id);
                if (kindergarten == null)
                {
                    TempData["ErrorMessage"] = String.Format("Садик с номером {0} отсутствует. Возможно, его удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                ViewBag.KindergartenId = Id;
                ViewBag.CanManageKindergarten = authz.Authorize(Operation.ManageKindergarten, kindergarten.Id);
                var users = (from usr in context.Users
                            join uk in context.UserKindergartens
                                on usr.Id equals uk.UserId
                            where uk.KindergartenId == Id
                            select usr).ToList();
                ViewBag.KindergartenUsers = users;
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View("View", kindergarten);
            }
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            using (var context = new SadikEntities())
            {
                var kindergarten = context.Kindergartens.FirstOrDefault(k => k.Id == Id);
                if (kindergarten == null)
                {
                    TempData["ErrorMessage"] = String.Format("Садик с номером {0} отсутствует. Возможно, его удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                ViewBag.KindergartenId = Id;
                if (!authz.Authorize(Operation.ManageKindergarten, kindergarten.Id))
                    return RedirectToAction("View", "Kindergarten", new {Id = Id });
                var model = new EditKindergartenModel()
                {
                    Id = kindergarten.Id,
                    Name = kindergarten.Name
                };

                ViewBag.CanManageEmployees = authz.Authorize(Operation.ManageEmployees, kindergarten.Id);
                ViewBag.CanManageKids = authz.Authorize(Operation.ManageKids, kindergarten.Id);
                ViewBag.CanAcceptKids = authz.Authorize(Operation.AcceptKids, kindergarten.Id);

                var users = (from usr in context.Users
                             join uk in context.UserKindergartens
                                 on usr.Id equals uk.UserId
                             where uk.KindergartenId == Id
                             select usr).ToList();
                ViewBag.KindergartenUsers = users;
                ViewBag.KindergartenId = Id;
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                var kids = context.Kids.Where(k => k.IsDismissed == false && k.KindergartenId == Id).ToList();
                ViewBag.KindergartenKids = kids;
                var formerkids = context.Kids.Where(k => k.IsDismissed == true && k.KindergartenId == Id).ToList();
                ViewBag.FormerKids = formerkids;

                return View("Edit", model);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditKindergartenModel model)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                var kindergarten = context.Kindergartens.FirstOrDefault(k => k.Id == model.Id);
                if (kindergarten == null)
                {
                    TempData["ErrorMessage"] = String.Format("Садик с номером {0} отсутствует. Возможно, его удалили.", model.Id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageKindergarten, kindergarten.Id))
                    throw new UnauthorizedAccessException("Редактировать садик могут только его сотрудники");
                kindergarten.Name = model.Name;

                context.SaveChanges();

                return RedirectToAction("View", "Kindergarten", new { Id = kindergarten.Id });
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            using (var context = new SadikEntities())
            {
                if (!authz.Authorize(Operation.CreateKindergartens))
                    throw new UnauthorizedAccessException("Вы не можете добавить новый садик");

                return View("Create", new CreateKindergartenModel());
            }
        }

        [HttpPost]
        public ActionResult Create(CreateKindergartenModel model)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                if (!authz.Authorize(Operation.CreateKindergartens))
                    throw new UnauthorizedAccessException("Вы не можете добавить новый садик");
                var kindergarten = new Kindergarten()
                {
                    Name = model.Name,
                    DateCreated = DateTime.Now,
                    IsDeleted = false
                };
                context.Kindergartens.Add(kindergarten);
                context.SaveChanges();

                CopyInventoryFromTemplate(kindergarten.Id);

                context.UserKindergartens.Add(new UserKindergarten() {
                    UserId = userSession.CurrentUser.Id,
                    KindergartenId = kindergarten.Id,
                    RoleId = UserRole.Teacher
                });
                context.SaveChanges();

                return RedirectToAction("View", "Kindergarten", new { Id = kindergarten.Id });
            }
        }

        private void CopyInventoryFromTemplate(int KindergartenId)
        {
            using (var context = new SadikEntities())
            {
                var templateZones = context.TemplateZones.ToList();
                var templateItems = context.TemplateInventories.ToList();
                var rootZone = templateZones.First(z => z.ParentZoneId == null);
                CopyZoneFromTemplate(KindergartenId, rootZone, null, templateZones, templateItems, context);
            }
        }

        private void CopyZoneFromTemplate(int KindergartenId, TemplateZone templateZone, int? parentZoneId, List<TemplateZone> templateZones, List<TemplateInventory> templateItems, SadikEntities context)
        {
            var newZone = new Zone()
            {
                Name = templateZone.Name,
                KindergartenId = KindergartenId,
                ParentZoneId = parentZoneId
            };
            var zone = context.Zones.Add(newZone);
            context.SaveChanges();
            var childItems = templateItems.Where(i => i.ParentZoneId == templateZone.Id).ToList();
            foreach (var childItem in childItems)
            {
                var newItem = new Inventory()
                {
                    Name = childItem.Name,
                    KindergartenId = KindergartenId,
                    ParentZoneId = zone.Id
                };
                context.Inventory.Add(newItem);
            }
            context.SaveChanges();
            var childZones = templateZones.Where(z => z.ParentZoneId == templateZone.Id).ToList();
            foreach (var childZone in childZones)
            {
                CopyZoneFromTemplate(KindergartenId, childZone, zone.Id, templateZones, templateItems, context);
            }
        }

        [HttpPost]
        public ActionResult AddUser(int KindergartenId, string userReference)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            using (var context = new SadikEntities())
            {
                var kindergarten = context.Kindergartens.FirstOrDefault(k => k.Id == KindergartenId);
                if (kindergarten == null)
                {
                    TempData["ErrorMessage"] = String.Format("Садик с номером {0} отсутствует. Возможно, его удалили.", KindergartenId);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageEmployees, kindergarten.Id))
                    throw new UnauthorizedAccessException("Вы не можете добавлять сотрудников в садик");

                User user = null;
                int userId = -1;
                var userReferenceLowerCase = userReference.ToLower();
                if (Regex.IsMatch(userReferenceLowerCase, SharedRegexPatterns.EmailPattern))
                {
                    user = context.Users.FirstOrDefault(u => u.Email == userReference);
                }
                else if (userReferenceLowerCase.Contains("/user/view/"))
                {
                    var urlPattern = ".*/user/view/([0-9]+).*";
                    var match = Regex.Match(userReferenceLowerCase, urlPattern);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        userId = int.Parse(match.Groups[1].Value);
                        user = context.Users.FirstOrDefault(u => u.Id == userId);
                    }
                }
                if (user == null)
                {
                    TempData["ErrorMessage"] = String.Format("Пользователь не найден");
                    return RedirectToAction("Edit", "Kindergarten", new { Id = kindergarten.Id });
                }
                userId = user.Id;
                if (context.UserKindergartens.Where(uk => uk.KindergartenId == KindergartenId && uk.UserId == userId).Any())
                {
                    TempData["ErrorMessage"] = String.Format("Пользователь уже работает в этом садике");
                    return RedirectToAction("Edit", "Kindergarten", new { Id = kindergarten.Id });
                }
                context.UserKindergartens.Add(new UserKindergarten() { UserId = user.Id, KindergartenId = KindergartenId, RoleId = UserRole.Teacher });
                context.SaveChanges();

                return RedirectToAction("Edit", "Kindergarten", new { Id = kindergarten.Id });
            }
        }

        [HttpPost]
        public ActionResult RemoveUser(int KindergartenId, int userId)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            using (var context = new SadikEntities())
            {
                if (!context.Kindergartens.Any(k => k.Id == KindergartenId))
                {
                    TempData["ErrorMessage"] = String.Format("Садик с номером {0} отсутствует. Возможно, его удалили.", KindergartenId);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageEmployees, KindergartenId))
                    throw new UnauthorizedAccessException("Вы не можете удалять сотрудников из садика");
                if (!context.Users.Any(k => k.Id == userId))
                {
                    TempData["ErrorMessage"] = String.Format("Пользователь с номером {0} отсутствует. Возможно, его удалили.", userId);
                    return RedirectToAction("Index", "Error");
                }

                var UKs = context.UserKindergartens.Where(uk => uk.KindergartenId == KindergartenId && uk.UserId == userId).ToList();
                UKs.ForEach(uk => context.UserKindergartens.Remove(uk));
                context.SaveChanges();

                return RedirectToAction("Edit", "Kindergarten", new { Id = KindergartenId });
            }
        }


        public KindergartenController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Kindergarten;
        }

        readonly IAuthorizationService authz;
    }
}
