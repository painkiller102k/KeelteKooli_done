using KeelteKooli.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace KeelteKooli.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Trainings()
        {
            var userId = User.Identity.GetUserId();

            var trainings = db.Trainings
                .Include(t => t.Keelekursus)
                .Include(t => t.Opetaja)
                .OrderByDescending(t => t.AlgusKuupaev)
                .ToList();

            var myIds = db.Registrations
                .Where(r => r.UserId == userId)
                .Select(r => r.KoolitusId)
                .ToList();

            var counts = db.Registrations
                .Where(r => r.Staatus != "Tühistatud")
                .GroupBy(r => r.KoolitusId)
                .Select(g => new { KoolitusId = g.Key, Cnt = g.Count() })
                .ToList()
                .ToDictionary(x => x.KoolitusId, x => x.Cnt);

            ViewBag.MyTrainingIds = myIds;
            ViewBag.Counts = counts;

            return View("~/Views/Student/Trainings.cshtml", trainings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int id) // id = Training.Id
        {
            var userId = User.Identity.GetUserId();

            var training = db.Trainings.Find(id);
            if (training == null) return HttpNotFound();

            bool already = db.Registrations.Any(r => r.UserId == userId && r.KoolitusId == id);
            if (already)
            {
                TempData["Error"] = "Sa oled juba sellele koolitusele registreeritud.";
                return RedirectToAction("Trainings");
            }

            // проверка заполненности группы
            int used = db.Registrations.Count(r => r.KoolitusId == id && r.Staatus != "Tühistatud"); // training 
            if (used >= training.MaxOsalejaid)
            {
                TempData["Error"] = "Vabandust, grupp on täis. Registreeruda ei saa.";
                return RedirectToAction("Trainings");
            }

            db.Registrations.Add(new Registration
            {
                UserId = userId,
                KoolitusId = id,
                Staatus = "Ootel"
            });
            db.SaveChanges();

            TempData["Message"] = "Registreerimine lisatud (staatus: Ootel).";
            return RedirectToAction("Trainings");
        }

        //Ootel/Kinnitatud/Tühistatud)
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