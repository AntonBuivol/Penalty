using Microsoft.AspNet.Identity;
using Penalty.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Penalty.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult ChangeLanguage(string lang)
        {
            Session["lang"] = lang;

            return RedirectToAction("Index");
        }

        public ActionResult Index(string searchCarNumber = null)
        {
            // Retrieve the penalties and include the related user
            var penalties = db.Penalty.Include(p => p.User).AsQueryable();

            // Если был введен номер машины для поиска (If the car number is entered for search)
            if (!string.IsNullOrEmpty(searchCarNumber))
            {
                penalties = penalties.Where(p => p.CarNumber.Contains(searchCarNumber));
            }

            // Если пользователь авторизован (If the user is authenticated), проверяем его роль (we check their role)
            if (User.Identity.IsAuthenticated)
            {
                var currentUserId = User.Identity.GetUserId();
                // You could further filter penalties based on user if required
            }

            // Retrieve the language from the session
            var lang = Session["lang"] as string;

            // Set the layout dynamically based on the language
            if (lang == "et")
            {
                ViewBag.Layout = "~/Views/Shared/_LayoutEST.cshtml";
                return View("IndexEST", penalties.ToList());  // Load the Estonian version of the Index view
            }
            else
            {
                ViewBag.Layout = "~/Views/Shared/_Layout.cshtml";  // Default layout
                return View("Index", penalties.ToList());  // Load the default Index view
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult Penalties(string searchCarNumber = null)
        {
            var penalties = db.Penalty.Include(p => p.User).AsQueryable();

            // Если был введен номер машины для поиска
            if (!string.IsNullOrEmpty(searchCarNumber))
            {
                penalties = penalties.Where(p => p.CarNumber.Contains(searchCarNumber));
            }

            // Если пользователь авторизован, проверяем его роль
            if (User.Identity.IsAuthenticated)
            {
                var currentUserId = User.Identity.GetUserId();

                // Если пользователь - администратор, он видит все штрафы
                if (User.IsInRole("Admin"))
                {
                    return View(penalties.ToList());
                }
                else
                {
                    // Если обычный пользователь, он видит только свои штрафы
                    penalties = penalties.Where(p => p.UserId == currentUserId);
                    return View(penalties.ToList());
                }
            }

            // Незарегистрированные пользователи видят все штрафы
            return View(penalties.ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Penalty_Create()
        {
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email");
            return View();

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Penalty_Create(PenaltyModel penalty)
        {
            if (ModelState.IsValid)
            {
                // Вычисляем сумму штрафа
                penalty.CalculateSumma();

                var user = db.Users.Find(penalty.UserId);
                penalty.User = user;
                E_mail(penalty);
                db.Penalty.Add(penalty);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email", penalty.UserId);
            return View(penalty);
        }

        [HttpGet]
        public ActionResult Penalty_Delete(int id)
        {
            PenaltyModel penalty = db.Penalty.Find(id);
            if (penalty == null)
            {
                return HttpNotFound();
            }
            return View(penalty);
        }

        [HttpPost, ActionName("Penalty_Delete")]
        public ActionResult Penalty_DeleteConfirmed(int id)
        {
            PenaltyModel penalty = db.Penalty.Find(id);
            if (penalty == null)
            {
                return HttpNotFound();
            }
            db.Penalty.Remove(penalty);
            db.SaveChanges();
            return RedirectToAction("Penalties");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Penalty_Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var penalty = db.Penalty.Include(p => p.User).FirstOrDefault(p => p.Id == id);
            if (penalty == null)
            {
                return HttpNotFound();
            }

            // Получаем список пользователей
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email", penalty.UserId);

            return View(penalty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Penalty_Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Penalty_EditConfirmed(PenaltyModel penalty)
        {
            if (ModelState.IsValid)
            {
                // Вычисляем сумму штрафа
                penalty.CalculateSumma();

                // Получаем пользователя по UserId и привязываем к штрафу
                var user = db.Users.Find(penalty.UserId);
                penalty.User = user;

                db.Entry(penalty).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Если что-то пошло не так, снова отображаем форму
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email", penalty.UserId);
            return View(penalty);
        }


        public void E_mail(PenaltyModel penalty)
        {
            var user = db.Users.Find(penalty.UserId);
            penalty.User = user;
            try
            {
                WebMail.SmtpServer = "smtp.gmail.com";
                WebMail.SmtpPort = 587;
                WebMail.EnableSsl = true;
                WebMail.UserName = "nepridumalnazvaniepocht@gmail.com";
                WebMail.Password = "rnlt mfvn ftjb usxu";
                WebMail.From = "nepridumalnazvaniepocht@gmail.com";
                WebMail.Send(user.Email, "Teil on uus Trahv!","Tere " + penalty.Name + " Auto number: " + penalty.CarNumber + " Trahv maksa: " 
                    + penalty.Summa + "Є" + " Trahvi kuupäev: " + penalty.Date.Year + "." + penalty.Date.Month + "." + penalty.Date.Day);
                ViewBag.Message = "Kiri on saatnud!";
            }
            catch
            {
                ViewBag.Message = "Mul on kahju! Ei saa kirja saada!!!";
            }
        }
    }
}