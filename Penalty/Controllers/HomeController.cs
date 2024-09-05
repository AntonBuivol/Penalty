using Penalty.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Penalty.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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

        public ActionResult Penalties()
        {
            var penalties = db.Penalty.Include(p => p.User).ToList();
            return View(penalties);
        }

        [HttpGet]
        public ActionResult Penalty_Create()
        {
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email");
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Penalty_Create(PenaltyModel penalty)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(penalty.UserId);
                penalty.User = user;

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

        [HttpGet]
        public ActionResult Penalty_Edit(int? id)
        {
            var penalty = db.Penalty.Find(id);
            if (penalty == null)
            {
                return HttpNotFound();
            }

            // Get the list of users
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email", penalty.UserId);

            return View(penalty);
        }

        [HttpPost, ActionName("Penalty_Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Penalty_EditConfirmed(PenaltyModel penalty)
        {
            if (ModelState.IsValid)
            {
                db.Entry(penalty).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "Email", penalty.UserId);
            return View(penalty);
        }
    }
}