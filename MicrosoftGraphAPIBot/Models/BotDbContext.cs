using Microsoft.EntityFrameworkCore;

namespace MicrosoftGraphAPIBot.Models
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        { 
        }

        public virtual DbSet<TelegramUser> TelegramUsers { get; set; }
        public virtual DbSet<AzureApp> AzureApps { get; set; }
        public virtual DbSet<AppAuth> AppAuths { get; set; }
    }
}
