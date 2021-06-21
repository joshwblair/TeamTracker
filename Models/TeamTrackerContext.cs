using Microsoft.EntityFrameworkCore;

namespace TeamTrackerApi.Models
{
    public class TeamTrackerContext : DbContext
    {
        public TeamTrackerContext(DbContextOptions<TeamTrackerContext> options): base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
    }
}