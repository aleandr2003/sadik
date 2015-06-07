using Sadik.Models;
using Sadik.Services;
using Sadik.Services.Abstract;
using Sadik.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class InventoryController : KindergartenDependentController
    {
        public ActionResult Index()
        {
            using (var context = new SadikEntities())
            {
                var zones = context.Zones.Include("ChildZones").Include("ParentZone")
                    .Include("Inventories").Where(z => z.KindergartenId == KindergartenId).ToList();
                var globalZone = zones.First(z => z.ParentZoneId == null);
                ViewBag.ZoneDropdownContent = GetZoneDropdownContent(globalZone);
                ViewBag.KindergartenId = KindergartenId;
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View("Index", globalZone);
            }
        }

        public ActionResult IndexShortJsonSerialized()
        {
            using (var context = new SadikEntities())
            {
                var items = context.Inventory.Where(i => !i.IsDeleted && i.KindergartenId == KindergartenId).ToList()
                    .OrderBy(i => i.Id)
                    .Select(i =>
                    {
                        var itemTitle = i.ZonePath.Count > 0 ? i.Name + "|" + String.Join("|", i.ZonePath.Select(z => z.Name).Reverse().ToArray()) : i.Name;
                        return new 
                        {
                            FullTitle = itemTitle,
                            Title = i.Name,
                            Id = i.Id.ToString()
                        };
                    }).ToList();
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                string sJSON = serializer.Serialize(items);// пришлось использовать такой хак, потому что если вставлять вызов на action возвращающий json в шаблон razor, подменяется content-type и браузер воспринимает всю страницу как json
                return Content(sJSON);
            }
        }

        public JsonResult IndexShortJson()
        {
            using (var context = new SadikEntities())
            {
                var items = context.Inventory.Where(i => !i.IsDeleted && i.KindergartenId == KindergartenId).ToList()
                    .Select(i =>
                    {
                        var itemTitle = i.ZonePath.Count > 0 ? i.Name + "|" + String.Join("|", i.ZonePath.Select(z => z.Name).Reverse().ToArray()) : i.Name;
                        return new
                        {
                            label = itemTitle,
                            value = i.Id.ToString()
                        };
                    }).ToList();
                return Json(items);
            }
        }

        [HttpGet]
        public ActionResult EditItem(int id)
        {
            using (var context = new SadikEntities())
            {
                var item = context.Inventory.FirstOrDefault(i => !i.IsDeleted && i.Id == id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = String.Format("Материал с номером {0} отсутствует. Возможно, его уже удалили.", id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageInvetoryItems, item))
                    throw new UnauthorizedAccessException("Редактировать материалы могут только воспитатели");

                var zones = context.Zones.Include("ChildZones").Include("ParentZone")
                    .Where(z => z.KindergartenId == item.KindergartenId).ToList();
                var globalZone = zones.First(z => z.ParentZoneId == null);
                ViewBag.ZoneDropdownContent = GetZoneDropdownContent(globalZone);
                
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View("EditItem", item);
            }
        }

        [HttpPost]
        public ActionResult EditItem(int id, string itemName, int parentZoneId)
        {
            using (var context = new SadikEntities())
            {
                var item = context.Inventory.FirstOrDefault(i => !i.IsDeleted && i.Id == id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = String.Format("Материал с номером {0} отсутствует. Возможно, его уже удалили.", id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageInvetoryItems, item))
                    throw new UnauthorizedAccessException("Редактировать материалы могут только воспитатели");

                if (context.Inventory.Any(i => !i.IsDeleted && i.ParentZoneId == parentZoneId && i.Name == itemName))
                {
                    TempData["ErrorMessage"] = itemName + " уже есть в этой зоне.";
                    return RedirectToAction("EditItem", "Inventory", new { id = id, KindergartenId = KindergartenId });
                }
                if (!context.Zones.Any(z => z.Id == parentZoneId && z.KindergartenId == item.KindergartenId))
                {
                    TempData["ErrorMessage"] = String.Format("Зона с номером {0} отсутствует. Возможно, её уже удалили.", parentZoneId);
                    return RedirectToAction("EditItem", "Inventory", new { id = id, KindergartenId = KindergartenId });
                }
                item.Name = itemName;
                item.ParentZoneId = parentZoneId;
                context.SaveChanges();
            }
            return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
        }

        [HttpPost]
        public ActionResult AddItem(string itemName, int parentZoneId)
        {
            if (!authz.Authorize(Operation.AddInvetoryItems, KindergartenId))
                throw new UnauthorizedAccessException("Добавлять новые материалы могут только воспитатели");
            using (var context = new SadikEntities())
            {
                if (context.Inventory.Any(i => !i.IsDeleted && i.ParentZoneId == parentZoneId && i.Name == itemName))
                {
                    TempData["ErrorMessage"] = itemName + " уже есть в этой зоне.";
                    return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
                }
                if (!context.Zones.Any(z => z.Id == parentZoneId))
                {
                    TempData["ErrorMessage"] = String.Format("Зона с номером {0} отсутствует. Возможно, её уже удалили.", parentZoneId);
                    return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
                }

                var item = new Inventory()
                {
                    Name = itemName,
                    ParentZoneId = parentZoneId,
                    IsDeleted = false,
                    KindergartenId = KindergartenId
                };
                context.Inventory.Add(item);
                context.SaveChanges();
            }
            return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
        }

        [HttpPost]
        public ActionResult RemoveItem(int Id)
        {
            using (var context = new SadikEntities())
            {
                var item = context.Inventory.FirstOrDefault(i => i.Id == Id);
                if (item == null) RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
                if (!authz.Authorize(Operation.ManageInvetoryItems, item))
                    throw new UnauthorizedAccessException("Удалять материалы могут только воспитатели");
                item.IsDeleted = true;
                context.SaveChanges();
            }
            return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
        }

        [HttpGet]
        public ActionResult AddZone()
        {
            using (var context = new SadikEntities())
            {
                if (!authz.Authorize(Operation.AddInvetoryItems, KindergartenId))
                    throw new UnauthorizedAccessException("Добавлять новые зоны могут только воспитатели");
                var zones = context.Zones.Include("ChildZones").Include("ParentZone").Where(z => z.KindergartenId == KindergartenId).ToList();
                var globalZone = zones.First(z => z.ParentZoneId == null);
                ViewBag.ZoneDropdownContent = GetZoneDropdownContent(globalZone);
                ViewBag.KindergartenId = KindergartenId;
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddZone(AddZoneModel model)
        {
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            if (!authz.Authorize(Operation.AddInvetoryItems, KindergartenId))
                throw new UnauthorizedAccessException("Добавлять новые зоны могут только воспитатели");
            using (var context = new SadikEntities())
            {
                if (model.parentZoneId != null && !context.Zones.Any(z => z.Id == model.parentZoneId && z.KindergartenId == KindergartenId))
                {
                    TempData["ErrorMessage"] = String.Format("Родительская зона с номером {0} отсутствует. Возможно, её уже удалили.", model.parentZoneId);
                    return RedirectToAction("Index", "Error");
                }
                if (context.Zones.Any(z => z.Name == model.Name && z.ParentZoneId == model.parentZoneId))
                {
                    TempData["ErrorMessage"] = model.Name + " уже есть в этой зоне.";
                    return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
                }
                var rootZone = context.Zones.First(z => z.ParentZoneId == null && z.KindergartenId == KindergartenId);
                var zone = new Zone()
                {
                    Name = model.Name,
                    ParentZoneId = model.parentZoneId ?? rootZone.Id,
                    KindergartenId = KindergartenId
                };
                context.Zones.Add(zone);
                context.SaveChanges();
            }
            return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId});
        }

        [HttpGet]
        public ActionResult EditZone(int Id)
        {
            using (var context = new SadikEntities())
            {
                var zone = context.Zones.FirstOrDefault(z => z.Id == Id);
                if (zone == null)
                {
                    TempData["ErrorMessage"] = String.Format("Зона с номером {0} отсутствует. Возможно, её уже удалили.", Id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageInvetoryItems, zone))
                    throw new UnauthorizedAccessException("Редактировать зоны могут только воспитатели");
                var zones = context.Zones.Include("ChildZones").Include("ParentZone").Where(z => z.KindergartenId == zone.KindergartenId).ToList();
                var globalZone = zones.First(z => z.ParentZoneId == null);
                ViewBag.ZoneDropdownContent = GetZoneDropdownContent(globalZone);
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                ViewBag.KindergartenId = zone.KindergartenId;
                var model = new EditZoneModel()
                {
                    Id = zone.Id,
                    Name = zone.Name,
                    parentZoneId = zone.ParentZoneId
                };
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditZone(EditZoneModel model)
        {
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                var zone = context.Zones.FirstOrDefault(z => z.Id == model.Id);
                if (zone == null)
                {
                    TempData["ErrorMessage"] = String.Format("Зона с номером {0} отсутствует. Возможно, её уже удалили.", model.Id);
                    return RedirectToAction("Index", "Error");
                }
                if (!authz.Authorize(Operation.ManageInvetoryItems, zone))
                    throw new UnauthorizedAccessException("Редактировать зоны могут только воспитатели");
                if (model.parentZoneId != null)
                {
                    var parentZone = context.Zones.FirstOrDefault(z => z.Id == model.parentZoneId);
                    if (parentZone == null)
                    {
                        TempData["ErrorMessage"] = String.Format("Родительская зона с номером {0} отсутствует. Возможно, её уже удалили.", model.parentZoneId);
                        return RedirectToAction("EditZone", "Inventory", new { KindergartenId = KindergartenId, Id = model.Id });
                    }
                    if (parentZone.Id == model.Id)
                    {
                        TempData["ErrorMessage"] = "Нельзя вложить зону в саму себя. Выберите другую родительскую зону";
                        return RedirectToAction("EditZone", "Inventory", new { KindergartenId = KindergartenId, Id = model.Id });
                    }
                    if (parentZone.ZonePath.Select(z => z.Id).ToList().Contains(model.Id))
                    {
                        TempData["ErrorMessage"] = String.Format("Родительская зона '{0}' вложена в текующую зону '{1}'. Выберите другую родительскую зону.", parentZone.Name, model.Name);
                        return RedirectToAction("EditZone", "Inventory", new { KindergartenId = KindergartenId, Id = model.Id });
                    }
                }
                if (context.Zones.Any(z => z.Name == model.Name && z.ParentZoneId == model.parentZoneId))
                {
                    TempData["ErrorMessage"] = model.Name + " уже есть в этой зоне.";
                    return RedirectToAction("EditZone", "Inventory", new { KindergartenId = KindergartenId, Id = model.Id });
                }
                var rootZone = context.Zones.First(z => z.ParentZoneId == null && z.KindergartenId == zone.KindergartenId);
                zone.Name = model.Name;
                zone.ParentZoneId = model.parentZoneId ?? rootZone.Id;

                context.SaveChanges();
            }
            return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
        }

        [HttpGet]
        public ActionResult RemoveZone(int Id)
        {
            using (var context = new SadikEntities())
            {
                var zone = context.Zones.FirstOrDefault(z => z.Id == Id);
                if (zone == null) RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
                if (!authz.Authorize(Operation.ManageInvetoryItems, zone))
                    throw new UnauthorizedAccessException("Удалять зоны могут только воспитатели");
                if (zone.ParentZoneId == null)
                    throw new UnauthorizedAccessException("Невозможно удалить корневую зону");

                var childItems = context.Inventory.Where(i => i.ParentZoneId == Id).ToList();
                var childZones = context.Zones.Where(i => i.ParentZoneId == Id).ToList();
                var parentZone = zone.ParentZone;//чтобы загрузить родительскую зону
                if (childItems.Any() || childZones.Any())
                {
                    return View(new Tuple<IEnumerable<Inventory>, IEnumerable<Zone>, Zone>(
                        childItems,
                        childZones,
                        zone));
                }
                else
                {
                    return RemoveZonePost(Id);
                }
            }
            
        }

        [HttpPost]
        public ActionResult RemoveZonePost(int Id)
        {
            using (var context = new SadikEntities())
            {
                var zone = context.Zones.FirstOrDefault(z => z.Id == Id);
                if (zone == null) RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
                if (!authz.Authorize(Operation.ManageInvetoryItems, zone))
                    throw new UnauthorizedAccessException("Удалять зоны могут только воспитатели");
                if (zone.ParentZoneId == null)
                    throw new UnauthorizedAccessException("Невозможно удалить корневую зону");

                var childItems = context.Inventory.Where(i => i.ParentZoneId == Id).ToList();
                var childZones = context.Zones.Where(i => i.ParentZoneId == Id).ToList();
                var globalZone = context.Zones.First(z => z.ParentZoneId == null && z.KindergartenId == zone.KindergartenId);
                childItems.ForEach(i => i.ParentZoneId = zone.ParentZoneId ?? globalZone.Id);
                childZones.ForEach(z => z.ParentZoneId = zone.ParentZoneId ?? globalZone.Id);
                context.Zones.Remove(zone);

                context.SaveChanges();
            }
            return RedirectToAction("Index", "Inventory", new { KindergartenId = KindergartenId });
        }

        public List<ZoneDropdownItem> GetZoneDropdownContent(Zone zone)
        {
            var childList = new List<ZoneDropdownItem>() { 
                new ZoneDropdownItem() { 
                    Name = zone.ParentZoneId == null ? "ГЛАВНАЯ" : zone.Name, 
                    Id = zone.Id 
                } 
            };
            if (zone.ChildZones != null && zone.ChildZones.Count > 0)
            {
                foreach (var childZone in zone.ChildZones)
                {
                    var childDropdownItems = GetZoneDropdownContentRecursive(childZone);
                    childList.AddRange(childDropdownItems);
                }
            }
            return childList;
        }

        public List<ZoneDropdownItem> GetZoneDropdownContentRecursive(Zone zone)
        {
            var childList = new List<ZoneDropdownItem>() { new ZoneDropdownItem() { Name = zone.Name, Id = zone.Id } };
            if (zone.ChildZones != null && zone.ChildZones.Count > 0)
            {
                foreach (var childZone in zone.ChildZones)
                {
                    var childDropdownItems = GetZoneDropdownContentRecursive(childZone);
                    for (int i = 0; i < childDropdownItems.Count; i++ )
                    {
                        var item = childDropdownItems[i];
                        item.Name = "--" + item.Name;
                        childDropdownItems[i] = item;
                    }
                    childList.AddRange(childDropdownItems);
                }
            }
            return childList;
        }

       /* public ActionResult CopyFromTemplate()
        {
            using (var context = new SadikEntities())
            {
                var templateZones = context.TemplateZones.ToList();
                var templateItems = context.TemplateInventories.ToList();
                var rootZone = templateZones.First(z => z.ParentZoneId == null);
                CopyZoneFromTemplate(rootZone, null, templateZones, templateItems, context);
            }
            return Content("OK");
        }

        private void CopyZoneFromTemplate(TemplateZone templateZone, int? parentZoneId, List<TemplateZone> templateZones, List<TemplateInventory> templateItems, SadikEntities context)
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
                CopyZoneFromTemplate(childZone, zone.Id, templateZones, templateItems, context);
            }
        }
        */

        public InventoryController(IUserSession userSession, IAuthorizationService authz)
            : base(userSession)
        {
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Inventory;
        }

        readonly IAuthorizationService authz;
    }
    public struct ZoneDropdownItem
    {
        public string Name;
        public int Id;
    }
}
