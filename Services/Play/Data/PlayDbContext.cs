using Microsoft.EntityFrameworkCore;
using Play.Models.PlayModel;

namespace Play.Data
{
    public class PlayDbContext : DbContext
    {
        public PlayDbContext(DbContextOptions<PlayDbContext> opt) : base(opt)
        {
        }

        public DbSet<PlayData> Plays { get; set; }
    }
}
