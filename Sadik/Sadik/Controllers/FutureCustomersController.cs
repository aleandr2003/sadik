using Sadik.Models;
using Sadik.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class FutureCustomersController : Controller
    {
        //
        // GET: /FutureCustomers/
        [HttpGet]
        public JsonResult IndexJson()
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    return Json(new { result = 0, schools = context
                        .FutureCustomers.Where(c => c.Latitude == null)
                        .Take(1000).ToList()}, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = 1, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddJson(AddFutureCustomerModel model)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    if (context.FutureCustomers.Any(c => c.PlaceId == model.PlaceId))
                        return Json(new { result = 0, PlaceId = model.PlaceId, message = "Place Id already exists" });
                    var futureCustomer = new FutureCustomer()
                    {
                        PlaceId = model.PlaceId,
                        Name = model.Name,
                        Country = model.Country,
                        State = model.State,
                        City = model.City,
                        Street = model.Street,
                        Building = model.Building,
                        Phone = model.Phone,
                        GoogleMapsUrl = model.GoogleMapsUrl,
                        Website = model.Website,
                        Email = model.Email,
                        Reference = model.Reference
                    };
                    context.FutureCustomers.Add(futureCustomer);
                    context.SaveChanges();
                }
                return Json(new { result = 0, PlaceId = model.PlaceId });
            }
            catch (Exception ex)
            {
                return Json(new { result = 1, message = ex.Message});
            }
        }

        [HttpPut]
        public JsonResult UpdateJson(AddFutureCustomerModel model)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var customer = context.FutureCustomers.FirstOrDefault(c => c.PlaceId == model.PlaceId);
                    if (customer == null)
                        return Json(new { result = 1, message = "Place Id doesn't exists" });
                    customer.Name = model.Name;
                    customer.Country = model.Country;
                    customer.State = model.State;
                    customer.City = model.City;
                    customer.Street = model.Street;
                    customer.Building = model.Building;
                    customer.Phone = model.Phone;
                    customer.GoogleMapsUrl = model.GoogleMapsUrl;
                    customer.Website = model.Website;
                    customer.Email = model.Email;
                    customer.Reference = model.Reference;
                    customer.Latitude = model.Latitude;
                    customer.Longitude = model.Longitude;

                    context.SaveChanges();
                }
                return Json(new { result = 0, PlaceId = model.PlaceId });
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    message += "; " + innerException.Message;
                    innerException = innerException.InnerException;
                }
                return Json(new { result = 1, message = ex.Message });
            }
        }

    }
}
