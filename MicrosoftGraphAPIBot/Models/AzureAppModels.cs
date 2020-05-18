using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicrosoftGraphAPIBot.Models
{
    public class AzureApp
    {
        public AzureApp()
        {
            Date = DateTime.Now;
            this.AppAuths = new List<AppAuth>();
        }

        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Secrets { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public virtual TelegramUser TelegramUser { get; set; }
        public ICollection<AppAuth> AppAuths { get; set; }
    }

    public class AppAuth
    {
        public AppAuth()
        {
            this.Id = Guid.NewGuid();
            this.Date = DateTime.Now;
        }

        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public virtual AzureApp AzureApp { get; set; }
    }
}
