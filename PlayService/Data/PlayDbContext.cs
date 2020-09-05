using Microsoft.EntityFrameworkCore;
using Play.Models.PlayModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Data
{
    public class PlayDbContext : DbContext
    {
        public PlayDbContext(DbContextOptions<PlayDbContext> opt) : base(opt)
        {
        }

        public DbSet<PlayData> Plays{ get; set; }
    }
}
