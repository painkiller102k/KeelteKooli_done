using KeelteKooli.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace KeelteKooli.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Все тренинги (можно записаться)
        public ActionResult Trainings()
        {
            var userId = User.Identity.GetUserId();

            var trainings = db.Trainings
                .Include(t => t.Keelekursus)
                .Include(t => t.Opetaja)
                .OrderByDescending(t => t.AlgusKuupaev)
                .ToList();

            // чтобы в View знать, на что уже записан
            var myIds = db.Registrations
                .Where(r => r.UserId == userId)
                .Select(r => r.KoolitusId)
                .ToList();

            ViewBag.MyTrainingIds = myIds;

            return View("~/Views/Student/Trainings.cshtml", trainings);
        }

        // Записаться на тренинг
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int id) // id = Training.Id
        {
            var userId = User.Identity.GetUserId();

            var training = db.Trainings.Find(id);
            if (training == null) return HttpNotFound();

            // чтобы не было дублей
            bool already = db.Registrations.Any(r => r.UserId == userId && r.KoolitusId == id);
            if (!already)
            {
                db.Registrations.Add(new Registration
                {
                    UserId = userId,
                    KoolitusId = id,
                    Staatus = "Ootel"
                });
                db.SaveChanges();
            }

            return RedirectToAction("Trainings");
        }

        // Мои тренинги (статус: Ootel/Kinnitatud/Tühistatud)
        public ActionResult MyTrainings()
        {
            var userId = User.Identity.GetUserId();

            var regs = db.Registrations
                .Include(r => r.Koolitus.Keelekursus)
                .Include(r => r.Koolitus.Opetaja)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();

            return View("~/Views/Student/MyTrainings.cshtml", regs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}