using Microsoft.EntityFrameworkCore;
using AdministrationPlat.Models; // 👈 Add this so it finds EventItem

namespace AdministrationPlat.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 👇 Match this to what your CalendarModel uses
        public DbSet<EventItem> TeacherEvents { get; set; }
        
        public DbSet<User> Users { get; set; }

    }
}