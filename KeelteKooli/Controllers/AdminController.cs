using KeelteKooli.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KeelteKooli.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager =>
            HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        // ---------------- COURSES ----------------
        public ActionResult Courses()
        {
            var courses = db.Courses.ToList();
            return View("Courses/Index", courses);
        }

        public ActionResult CreateCourse()
        {
            return View("Courses/Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourse(Course course)
        {
            if (!ModelState.IsValid)
                return View("Courses/Create", course);

            db.Courses.Add(course);
            db.SaveChanges();
            return RedirectToAction(nameof(Courses));
        }

        public ActionResult EditCourse(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = db.Courses.Find(id);
            if (course == null) return HttpNotFound();

            return View("Courses/Edit", course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse(Course course)
        {
            if (!ModelState.IsValid)
                return View("Courses/Edit", course);

            db.Entry(course).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction(nameof(Courses));
        }

        public ActionResult DeleteCourse(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = db.Courses.Find(id);
            if (course == null) return HttpNotFound();

            return View("Courses/Delete", course);
        }

        [HttpPost, ActionName("DeleteCourse")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCourseConfirmed(int id)
        {
            var course = db.Courses.Find(id);
            if (course == null) return HttpNotFound();

            db.Courses.Remove(course);
            db.SaveChanges();
            return RedirectToAction(nameof(Courses));
        }

        // ---------------- TEACHERS (OPETAJA) ----------------
        public ActionResult Teachers()
        {
            var teachers = db.Teachers.Include(t => t.User).ToList();
            return View("Opetaja/Index", teachers);
        }

        public ActionResult CreateTeacher()
        {
            return View("Opetaja/Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTeacher(Teacher teacher, string Password)
        {
            if (string.IsNullOrWhiteSpace(teacher.Email))
                ModelState.AddModelError("Email", "Email is required");

            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError("Password", "Password is required");

            if (!ModelState.IsValid)
                return View("Opetaja/Create", teacher);

            // создаём Identity user
            var newUser = new ApplicationUser
            {
                UserName = teacher.Email,
                Email = teacher.Email
            };

            var result = UserManager.Create(newUser, Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", string.Join(", ", result.Errors));
                return View("Opetaja/Create", teacher);
            }

            // роль учителя
            UserManager.AddToRole(newUser.Id, "Opetaja");

            // сохраняем teacher + связь с ApplicationUser
            teacher.ApplicationUserId = newUser.Id;
            db.Teachers.Add(teacher);
            db.SaveChanges();

            return RedirectToAction(nameof(Teachers));
        }

        public ActionResult EditTeacher(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var teacher = db.Teachers.Find(id);
            if (teacher == null) return HttpNotFound();

            return View("Opetaja/Edit", teacher);
        }

        // ⚠️ Пароль меняем без токенов: RemovePassword + AddPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTeacher(Teacher teacher, string NewPassword)
        {
            if (string.IsNullOrWhiteSpace(teacher.Email))
                ModelState.AddModelError("Email", "Email is required");

            if (!ModelState.IsValid)
                return View("Opetaja/Edit", teacher);

            var dbTeacher = db.Teachers.Find(teacher.Id);
            if (dbTeacher == null) return HttpNotFound();

            // обновляем данные teacher
            dbTeacher.Nimi = teacher.Nimi;
            dbTeacher.Kvalifikatsioon = teacher.Kvalifikatsioon;
            dbTeacher.Email = teacher.Email;

            // обновляем Identity user
            if (!string.IsNullOrEmpty(dbTeacher.ApplicationUserId))
            {
                var user = UserManager.FindById(dbTeacher.ApplicationUserId);
                if (user != null)
                {
                    user.Email = teacher.Email;
                    user.UserName = teacher.Email;

                    var upd = UserManager.Update(user);
                    if (!upd.Succeeded)
                    {
                        ModelState.AddModelError("", string.Join(", ", upd.Errors));
                        return View("Opetaja/Edit", teacher);
                    }

                    // если ввели новый пароль — меняем
                    if (!string.IsNullOrWhiteSpace(NewPassword))
                    {
                        // снимаем старый, ставим новый (без TokenProvider)
                        if (UserManager.HasPassword(user.Id))
                        {
                            var rem = UserManager.RemovePassword(user.Id);
                            if (!rem.Succeeded)
                            {
                                ModelState.AddModelError("", string.Join(", ", rem.Errors));
                                return View("Opetaja/Edit", teacher);
                            }
                        }

                        var add = UserManager.AddPassword(user.Id, NewPassword);
                        if (!add.Succeeded)
                        {
                            ModelState.AddModelError("", string.Join(", ", add.Errors));
                            return View("Opetaja/Edit", teacher);
                        }
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction(nameof(Teachers));
        }

        public ActionResult DeleteTeacher(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var teacher = db.Teachers.Include(t => t.User).FirstOrDefault(t => t.Id == id);
            if (teacher == null) return HttpNotFound();

            return View("Opetaja/Delete", teacher);
        }

        [HttpPost, ActionName("DeleteTeacher")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTeacherConfirmed(int id)
        {
            var teacher = db.Teachers.Find(id);
            if (teacher == null) return HttpNotFound();

            // удалить trainings этого учителя
            var trainings = db.Trainings.Where(t => t.OpetajaId == teacher.Id).ToList();
            if (trainings.Any())
                db.Trainings.RemoveRange(trainings);

            // удалить registrations на этих trainings (чтобы не было FK ошибок)
            var regToDelete = db.Registrations.Where(r => r.Koolitus.OpetajaId == teacher.Id).ToList();
            if (regToDelete.Any())
                db.Registrations.RemoveRange(regToDelete);

            // удалить Teacher
            db.Teachers.Remove(teacher);
            db.SaveChanges();

            // удалить Identity user корректно (через UserManager, найденный заново)
            if (!string.IsNullOrEmpty(teacher.ApplicationUserId))
            {
                var user = UserManager.FindById(teacher.ApplicationUserId);
                if (user != null)
                {
                    var roles = UserManager.GetRoles(user.Id).ToList();
                    foreach (var r in roles)
                        UserManager.RemoveFromRole(user.Id, r);

                    UserManager.Delete(user);
                }
            }

            return RedirectToAction(nameof(Teachers));
        }

        // ---------------- TRAININGS ----------------
        public ActionResult Trainings()
        {
            var trainings = db.Trainings
                .Include(t => t.Keelekursus)
                .Include(t => t.Opetaja)
                .ToList();

            return View("Trainings/Index", trainings);
        }

        public ActionResult CreateTraining()
        {
            ViewBag.Courses = new SelectList(db.Courses, "Id", "Nimetus");
            ViewBag.Teachers = new SelectList(db.Teachers, "Id", "Nimi");
            return View("Trainings/Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTraining(Training training)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(db.Courses, "Id", "Nimetus", training.KeelekursusId);
                ViewBag.Teachers = new SelectList(db.Teachers, "Id", "Nimi", training.OpetajaId);
                return View("Trainings/Create", training);
            }

            db.Trainings.Add(training);
            db.SaveChanges();
            return RedirectToAction(nameof(Trainings));
        }

        public ActionResult EditTraining(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var training = db.Trainings.Find(id);
            if (training == null) return HttpNotFound();

            ViewBag.Courses = new SelectList(db.Courses, "Id", "Nimetus", training.KeelekursusId);
            ViewBag.Teachers = new SelectList(db.Teachers, "Id", "Nimi", training.OpetajaId);
            return View("Trainings/Edit", training);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTraining(Training training)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(db.Courses, "Id", "Nimetus", training.KeelekursusId);
                ViewBag.Teachers = new SelectList(db.Teachers, "Id", "Nimi", training.OpetajaId);
                return View("Trainings/Edit", training);
            }

            db.Entry(training).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction(nameof(Trainings));
        }

        public ActionResult DeleteTraining(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var training = db.Trainings
                .Include(t => t.Keelekursus)
                .Include(t => t.Opetaja)
                .FirstOrDefault(t => t.Id == id);

            if (training == null) return HttpNotFound();

            return View("Trainings/Delete", training);
        }

        [HttpPost, ActionName("DeleteTraining")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTrainingConfirmed(int id)
        {
            // удалить registrations этого тренинга (иначе FK ошибка)
            var regs = db.Registrations.Where(r => r.KoolitusId == id).ToList();
            if (regs.Any())
                db.Registrations.RemoveRange(regs);

            var training = db.Trainings.Find(id);
            if (training == null) return HttpNotFound();

            db.Trainings.Remove(training);
            db.SaveChanges();
            return RedirectToAction(nameof(Trainings));
        }

        // ---------------- REGISTRATIONS (STAATUS) ----------------
        public ActionResult Registrations()
        {
            var regs = db.Registrations
                .Include(r => r.User)
                .Include(r => r.Koolitus.Keelekursus)
                .Include(r => r.Koolitus.Opetaja)
                .OrderByDescending(r => r.Id)
                .ToList();

            return View("~/Views/Admin/Registrations/Index.cshtml", regs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmRegistration(int id)
        {
            var reg = db.Registrations.Find(id);
            if (reg == null) return HttpNotFound();

            reg.Staatus = "Kinnitatud";
            db.SaveChanges();
            return RedirectToAction(nameof(Registrations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelRegistration(int id)
        {
            var reg = db.Registrations.Find(id);
            if (reg == null) return HttpNotFound();

            reg.Staatus = "Tühistatud";
            db.SaveChanges();
            return RedirectToAction(nameof(Registrations));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}