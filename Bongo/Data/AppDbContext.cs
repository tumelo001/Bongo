using Bongo.Models;
using Bongo.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bongo.Data
{
    public class AppDbContext : IdentityDbContext<BongoUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<Timetable> Timetables { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<ModuleColor> ModuleColors { get; set; }

        public DbSet<UserReview> UserReviews { get; set; }  
    }
}
