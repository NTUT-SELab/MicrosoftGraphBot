using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MicrosoftGraphAPIBot.Models
{
    public class AzureApp
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Secrets { get; set; }
        [Required]
        public virtual TelegramUser TelegramUser { get; set; }
    }
}
