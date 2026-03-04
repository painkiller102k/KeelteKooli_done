using KeelteKooli.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations;
using System.Linq;

namespace KeelteKooli.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<KeelteKooli.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(KeelteKooli.Models.ApplicationDbContext context)
        {
            // ---------------- ROLES ----------------
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            string[] roles = { "Admin", "Opetaja", "Student" };

            foreach (var roleName in roles)
            {
                if (!roleManager.RoleExists(roleName))
                    roleManager.Create(new IdentityRole(roleName));
            }

            // ---------------- ADMIN ----------------
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            string adminEmail = "admin@gmail.com";
            var adminUser = userManager.FindByEmail(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };
                userManager.Create(adminUser, "Admin123!");
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            context.SaveChanges();
        }
    }
}