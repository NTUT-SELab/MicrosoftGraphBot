﻿using Microsoft.EntityFrameworkCore;

namespace MicrosoftGraphAPIBot.Models
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        { 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AzureApp>()
                .HasOne(app => app.TelegramUser)
                .WithMany(user => user.AzureApps)
                .HasForeignKey(app => app.TelegramUserId);

            modelBuilder.Entity<AppAuth>()
                .HasOne(auth => auth.AzureApp)
                .WithMany(app => app.AppAuths)
                .HasForeignKey(auth => auth.AzureAppId);
        }

        public virtual DbSet<TelegramUser> TelegramUsers { get; set; }
        public virtual DbSet<AzureApp> AzureApps { get; set; }
        public virtual DbSet<AppAuth> AppAuths { get; set; }
    }
}
