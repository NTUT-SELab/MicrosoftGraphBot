using System.ComponentModel.DataAnnotations;

namespace MicrosoftGraphAPIBot.Models
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
    }
}
