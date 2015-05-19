using Sadik.Models;
using Sadik.Models.Notifications;
using Sadik.Services.Abstract;
using Sadik.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class LoginController : SadikController
    {
        INotificationSender sender;
        public LoginController(IUserSession userSession, INotificationSender sender)
            : base(userSession)
        {
            this.sender = sender;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult Index(string ErrorMessage, string returnUrl)
        {
            if (userSession.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ErrorMessage = ErrorMessage;
            ViewBag.returnUrl = returnUrl;
            return View(new LoginModel());
        }

        [HttpPost]
        public ActionResult LogIn(LoginModel form, string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl)) returnUrl = Url.Action("Index", "Home");
            User currentUser = null;
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                currentUser = userSession.Authenticate(form.Email, form.Password ?? "");
            }
            catch (ArgumentException ex)
            {
                return RedirectToAction("Index", "Login", new { ErrorMessage = ex.Message });
            }
            userSession.LogIn(currentUser);
            return Redirect(returnUrl);
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View("ForgotPassword");
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            try
            {
                using (var context = new SadikEntities())
                {
                    var user = context.Users.Where(u => u.Email == Email).FirstOrDefault();
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "У нас нет пользователя с таким емейлом. Попросите администратора создать вам аккаунт";
                        return RedirectToAction("ForgotPassword");
                    }
                    var newPassword = Password.GenerateRandom();
                    user.SetPassword(newPassword);
                    sender.Send(new PasswordResetNotificationTemplate(
                        user.Email,
                        user.FirstName,
                        newPassword));

                    context.SaveChanges();
                }
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ForgotPassword");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ForgotPassword");
            }
            return RedirectToAction("PasswordReset");
        }

        [HttpGet]
        public ActionResult PasswordReset()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOut()
        {
            userSession.LogOut();
            return RedirectToAction("Index", "Home");
        }

    }
}
