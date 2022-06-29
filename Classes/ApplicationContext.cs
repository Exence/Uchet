using System.Data.Entity;

namespace Uchet.Classes
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<MainUser> MainUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Team> Teams { get; set; }

        public ApplicationContext() : base("DefaultConnection") { }

    }
}
