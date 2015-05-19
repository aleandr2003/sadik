using Sadik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class StatAreaController : Controller
    {
        [HttpGet]
        public JsonResult IndexJson()
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var cities = context.Cities.Where(c => !c.IsSearched).Select(c =>
                            new
                            {
                                c.Country,
                                c.Id,
                                c.Latitude,
                                c.Longitude,
                                c.Name,
                                c.Population,
                                c.IsSearched
                            }).ToList();
                    return Json(new
                    {
                        result = 0,
                        areas = cities
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = 1, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SetSearchedJson(int Id, bool searched)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var area = context.Cities.FirstOrDefault(c => c.Id == Id);
                    if (area == null)
                        return Json(new { result = 1, message = "Area Id doesn't exists" });
                    area.IsSearched = searched;
                    context.SaveChanges();
                }
                return Json(new { result = 0, Id = Id, IsSearched = searched });
            }
            catch (Exception ex)
            {
                return Json(new { result = 1, message = ex.Message });
            }
        }


    }
}
