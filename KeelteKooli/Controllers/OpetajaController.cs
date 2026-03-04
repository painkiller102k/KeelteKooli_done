using KeelteKooli.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace KeelteKooli.Controllers
{
    [Authorize(Roles = "Opetaja")]
    public class OpetajaController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Dashboard преподавателя: список его тренингов
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();

            var teacher = db.Teachers.FirstOrDefault(t => t.ApplicationUserId == userId);
            if (teacher == null) return HttpNotFound();

            var trainings = db.Trainings
                .Include(t => t.Keelekursus)
                .Where(t => t.OpetajaId == teacher.Id)
                .OrderByDescending(t => t.AlgusKuupaev)
                .ToList();

            return View("~/Views/Admin/Opetaja/Dashboard.cshtml", trainings);
        }

        // Студенты конкретного тренинга (только если тренинг принадлежит этому учителю)
        public ActionResult Students(int? id) // <-- nullable
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();

            var teacher = db.Teachers.FirstOrDefault(t => t.ApplicationUserId == userId);
            if (teacher == null) return HttpNotFound();

            // проверка доступа: тренинг должен принадлежать этому учителю
            var training = db.Trainings
                .Include(t => t.Keelekursus)
                .FirstOrDefault(t => t.Id == id.Value && t.OpetajaId == teacher.Id);

            if (training == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var registrations = db.Registrations
                .Include(r => r.User)
                .Include(r => r.Koolitus.Keelekursus)
                .Where(r => r.KoolitusId == id.Value)
                .OrderByDescending(r => r.Id)
                .ToList();

            return View("~/Views/Admin/Opetaja/Students.cshtml", registrations);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}