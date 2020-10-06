using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicrosoftGraphAPIBot.Models
{
    public class AzureApp
    {
        public AzureApp()
        {
            RegTime = DateTime.Now;
            this.AppAuths = new List<AppAuth>();
        }

        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Secrets { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public DateTime RegTime { get; set; }


        [Required]
        public long TelegramUserId { get; set; }
        [Required]
        public virtual TelegramUser TelegramUser { get; set; }
        public ICollection<AppAuth> AppAuths { get; set; }
    }

    public class AppAuth
    {
        public AppAuth()
        {
            this.Id = Guid.NewGuid();
            this.BindTime = DateTime.Now;
            this.UpdateTime = DateTime.Now;
        }

        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public string Scope { get; set; }
        [Required]
        public DateTime BindTime { get; set; }
        [Required]
        public DateTime UpdateTime { get; set; }

        [Required]
        public Guid AzureAppId { get; set; }
        [Required]
        public virtual AzureApp AzureApp { get; set; }
    }

    public class ApiResult
    {
        public ApiResult()
        {
            Id = Guid.NewGuid();
            Date = DateTime.Now;
        }

        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Result { get; set; }

        [Required]
        public long TelegramUserId { get; set; }
        [Required]
        public virtual TelegramUser TelegramUser { get; set; }
    }
}
