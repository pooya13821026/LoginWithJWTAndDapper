using System.ComponentModel.DataAnnotations;

namespace ramand.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
        public string VerificationToken { get; set; } = string.Empty;
        public DateTime ResetTokenExpires { get; set; }
    }
}
