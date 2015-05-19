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
    public class UserController : SadikController
    {

        public ActionResult Index()
        {
            using (var context = new SadikEntities())
            {
                var users = context.Users.Where(u => !u.IsDeleted).ToList();
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                return View("Index", users);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (!authz.Authorize(Operation.CreateUsers))
                throw new UnauthorizedAccessException("Вы не можете добавлять новых пользователей");
            if (userSession.IsAuthenticated && userSession.CurrentUser.RoleId != UserRole.Admin)
            {
                return Redirect(Url.Action("View", "User", new { id = userSession.CurrentUser.Id }));
            }
            return View(new RegisterModel());
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model, string returnUrl)
        {
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            if (!authz.Authorize(Operation.CreateUsers))
                throw new UnauthorizedAccessException("Вы не можете добавлять новых пользователей");
            using (var context = new SadikEntities())
            {
                try
                {
                    if (context.Users.Any(u => u.Email == model.Email))
                        throw new ArgumentException("Пользователь с таким емейлом уже существует");
                    var user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email
                    };
                    user.SetPassword(model.Password);

                    context.Users.Add(user);
                    context.SaveChanges();
                    if (!userSession.IsAuthenticated)
                    {
                        userSession.LogIn(user);
                        if (String.IsNullOrEmpty(returnUrl)) returnUrl = Url.Action("View", "User", new { id = user.Id });
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "User");
                    }
                }
                catch (Exception ex)
                {
                    if (ex is UpdateException ||
                        ex is ArgumentException)
                    {
                        return RedirectToAction("Index", "Error", new { message = "Пользователь с таким емейлом уже существует" });
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult RemoveUser(int Id)
        {
            using (var context = new SadikEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == Id);
                if (user == null) throw new ArgumentException(String.Format("Пользователь с номером {0} отсутствует", Id));
                if (!authz.Authorize(Operation.RemoveUsers, user))
                    throw new UnauthorizedAccessException("Удалять пользователей могут только администраторы");
                user.IsDeleted = true;
                context.SaveChanges();
            }
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public ActionResult View(int Id)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewData["SelectedTab"] = HeaderTabs.MyProfile;
            using (var context = new SadikEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == Id);
                if (user == null) throw new ArgumentException(String.Format("Пользователь с номером {0} отсутствует", Id));
                return View("View", user);
            }
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewData["SelectedTab"] = HeaderTabs.MyProfile;
            using (var context = new SadikEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                var model = new EditProfileModel()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return View("EditProfile", model);
            }
        }

        [HttpPost]
        public ActionResult EditProfile(EditProfileModel model)
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SadikEntities())
            {
                //if (context.Users.Any(u => u.Email == model.Email && u.Id != userSession.CurrentUser.Id))
                //{
                //    TempData["ErrorMessage"] = "Пользователь с таким емейлом уже существует";
                //    return RedirectToAction("EditProfile", "User");
                //}
                var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                //user.Email = model.Email;
                context.SaveChanges();

                return RedirectToAction("View", "User", new { Id = user.Id });
            }
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewData["SelectedTab"] = HeaderTabs.MyProfile;
            return View("ChangePassword");
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    TempData["ErrorMessage"] = "Повторный ввод не совпадает с первым. Попробуйте ещё раз";
                    return RedirectToAction("ChangePassword", "User");
                }
                if (!userSession.IsAuthenticated) return RedirectToAction("Index", "Login");

                using (var context = new SadikEntities())
                {
                    var user = context.Users.Where(u => u.Id == userSession.CurrentUser.Id).FirstOrDefault();
                    if (!userSession.CurrentUser.PasswordMatches(model.OldPassword))
                    {
                        TempData["ErrorMessage"] = "Текущий пароль введен не верно";
                        return RedirectToAction("ChangePassword", "User");
                    }
                    user.SetPassword(model.NewPassword, model.OldPassword);
                    context.SaveChanges();
                }
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("ChangePassword");
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("ChangePassword");
            }
            return RedirectToAction("View", "User", new { Id = userSession.CurrentUser.Id });
        }

        public UserController(IUserSession userSession, IAuthorizationService authz/*, AvatarService avatarService*/)
            : base(userSession)
        {
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Admin;
        }

       // private readonly AvatarService avatarService;
        readonly IAuthorizationService authz;
    }
}
