using System.ComponentModel.DataAnnotations;

namespace CLDV6212P1.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

}
