using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftGraphAPIBot.Models
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        { 
        }

        public virtual DbSet<User> Users { get; set; }
    }
}
