using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicrosoftGraphAPIBot.Models
{
    public class TelegramUser
    {
        public TelegramUser()
        {
            IsAdmin = false;
            this.AzureApps = new List<AzureApp>();
        }

        [Key]
        [Required]
        public long Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
        public ICollection<AzureApp> AzureApps { get; set; }
    }
}
