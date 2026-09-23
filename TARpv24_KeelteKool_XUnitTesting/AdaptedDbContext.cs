using KeelteKooli.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace TARpv24_KeelteKool_XUnitTesting
{
    public class AdaptedDbContext : IdentityDbContext<ApplicationUser>
    {
        public AdaptedDbContext() : base ("DefaultConnection")
    }
}
