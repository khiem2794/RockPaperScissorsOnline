using Microsoft.EntityFrameworkCore;
using PlayService.Models.PlayModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayService.Data
{
    public class PlayDbContext : DbContext
    {
        public PlayDbContext(DbContextOptions<PlayDbContext> opt) : base(opt)
        {
        }

        public DbSet<PlayData> Plays{ get; set; }
    }
}
